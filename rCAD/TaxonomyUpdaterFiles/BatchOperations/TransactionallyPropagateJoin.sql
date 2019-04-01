/* 
* Copyright (c) 2009, The University of Texas at Austin
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without modification, 
* are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice, 
* this list of conditions and the following disclaimer.
*
* 2. Redistributions in binary form must reproduce the above copyright notice, 
* this list of conditions and the following disclaimer in the documentation and/or other materials 
* provided with the distribution.
*
* Neither the name of The University of Texas at Austin nor the names of its contributors may be 
* used to endorse or promote products derived from this software without specific prior written 
* permission.
* 
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
* FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS 
* BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF 
* THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
GO
/*Do this work inside of a transaction to avoid any modifications to the SequenceMain table while the update
occurs and to have automatic rollback if any statement fails within the block*/
BEGIN TRANSACTION TaxonomyUpdate

BEGIN TRY

INSERT INTO RNAJOINOrphanedSequences (SeqID, OldTaxID)
SELECT SM.SeqID, SM.TaxID FROM SequenceMain AS SM
INNER JOIN RNAJOINDeletedNodes
ON SM.TaxID=RNAJOINDeletedNodes.TaxID;

INSERT INTO RNAJOINMergedSequences(SeqID, NewTaxID, OldTaxID)
SELECT SM.SeqID, RNAJOINMergedNodes.NewTaxID, SM.TaxID
FROM SequenceMain AS SM
INNER JOIN RNAJOINMergedNodes
ON SM.TaxID=RNAJOINMergedNodes.TaxID;

INSERT INTO RNAJOINMovedSequences(SeqID, TaxID, OldParentTaxID, NewParentTaxID)
SELECT SM.SeqID, SM.TaxID, RNAJOINMovedNodes.ParentTaxID, NCBITempNodes.ParentTaxID
FROM SequenceMain AS SM
INNER JOIN RNAJOINMovedNodes
ON SM.TaxID=RNAJOINMovedNodes.TaxID
INNER JOIN NCBITempNodes
ON SM.TaxID=NCBITempNodes.TaxID

/*Check if any nodes were "orphaned" in the NCBI update process we
we keep them until the next update*/

IF(SELECT COUNT(*) FROM RNAJOINTaxonomyOuterNodes) > 0
BEGIN
	INSERT INTO RNAJOINTaxonomyMerged (TaxID, ParentTaxID)
	SELECT TN.* FROM RNAJOINTaxonomyOuterNodes AS TN;
END

INSERT INTO RNAJOINTaxonomyArchived (TaxID, ParentTaxID)
SELECT T.* FROM Taxonomy AS T;
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Taxonomy') AND name = 'IX_Taxonomy_ParentTaxID')
DROP INDEX IX_Taxonomy_ParentTaxID ON Taxonomy WITH ( ONLINE = OFF );
TRUNCATE TABLE Taxonomy;
INSERT INTO Taxonomy (TaxID, ParentTaxID)
SELECT RTM.* FROM RNAJOINTaxonomyMerged AS RTM;
CREATE NONCLUSTERED INDEX IX_Taxonomy_ParentTaxID ON Taxonomy 
(
	[ParentTaxID] ASC
)WITH (PAD_INDEX  = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF)

INSERT INTO RNAJOINTaxonomyNamesArchived (TaxID, ScientificName)
SELECT TN.* FROM TaxonomyNames AS TN;
TRUNCATE TABLE TaxonomyNames;
TRUNCATE TABLE TaxonomyNamesOrdered;
INSERT INTO TaxonomyNames (TaxID, ScientificName)
SELECT RTN.* FROM RNAJOINTaxonomyNames AS RTN;

INSERT INTO RNAJOINAlternateNamesArchived (TaxID, NameClassID, Name)
SELECT AN.* FROM AlternateNames AS AN;
TRUNCATE TABLE AlternateNames;
INSERT INTO AlternateNames (TaxID, NameClassID, Name)
SELECT RAN.* FROM RNAJOINAlternateNames AS RAN;

INSERT INTO RNAJOINNameClassesArchived (NameClassID, NameClass)
SELECT NC.* FROM NameClasses AS NC;
TRUNCATE TABLE NameClasses;
INSERT INTO NameClasses (NameClassID, NameClass)
SELECT RNC.* FROM RNAJOINNameClasses AS RNC;

UPDATE SequenceMain
SET TaxID=(SELECT MS.NewTaxID FROM RNAJOINMergedSequences AS MS
WHERE MS.SeqID=SequenceMain.SeqID)
WHERE SeqID IN (SELECT SeqID FROM RNAJOINMergedSequences);

UPDATE SequenceMain
SET TaxID=-1
WHERE SeqID IN (SELECT OS.SeqID FROM RNAJOINOrphanedSequences AS OS);

END TRY

BEGIN CATCH

ROLLBACK TRANSACTION TaxonomyUpdate
RETURN

END CATCH

COMMIT TRANSACTION TaxonomyUpdate