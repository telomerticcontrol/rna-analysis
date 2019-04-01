bcp %1.dbo.RNAJOINOrphanedSequences out %2.RNAJOINOrphanedSequences.dmp -T -c -S localhost
bcp %1.dbo.RNAJOINMergedSequences out %2.RNAJOINMergedSequences.dmp -T -c -S localhost
bcp %1.dbo.RNAJOINMovedSequences out %2.RNAJOINMovedSequences.dmp -T -c -S localhost