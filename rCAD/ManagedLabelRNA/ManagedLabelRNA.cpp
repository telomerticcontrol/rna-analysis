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

#include "stdafx.h"

#include "ManagedLabelRNA.h"

ManagedLabelRNA::ManagedRNA::ManagedRNA(System::Collections::Generic::Dictionary<int,int>^ pairs, int sequenceLength)
{
	pUnmanagedRNA = new RNA();

	System::Collections::Generic::Dictionary<int, int>^ pairsReverse = gcnew System::Collections::Generic::Dictionary<int, int>();

	for(int i=1; i<=sequenceLength; i++)
	{
		if(pairs->ContainsKey(i))
		{
			pUnmanagedRNA->bindings.add(pairs[i]-1);
			pairsReverse->Add(pairs[i], i);
		}
		else if(pairs->ContainsValue(i))
		{
			pUnmanagedRNA->bindings.add(pairsReverse[i]-1);
		}
		else
		{
			pUnmanagedRNA->bindings.add(-1);
		}
	}
}

ManagedLabelRNA::ManagedRNA::ManagedRNA(const ManagedRNA^ me)
{
	pUnmanagedRNA = me->pUnmanagedRNA;
}

System::Xml::Linq::XElement^ ManagedLabelRNA::ManagedRNA::ComputeTieredAsXML()
{
	if(pUnmanagedRNA->computeTiered())
	{
		return GetStructureElementsXML();
	}
	else
	{
		return gcnew System::Xml::Linq::XElement("StructureExtents"); 
	}
}

System::Collections::Generic::Dictionary<System::String^, System::String^>^ ManagedLabelRNA::ManagedRNA::ComputeTiered()
{
	if(pUnmanagedRNA->computeTiered())
	{
		return GetStructureElements();
	}
	else
	{
		return gcnew System::Collections::Generic::Dictionary<System::String^, System::String^>();
	}
	
}

System::Xml::Linq::XElement^ ManagedLabelRNA::ManagedRNA::GetStructureElementsXML()
{
	System::Collections::Generic::List<String^>^ extentIDs = gcnew System::Collections::Generic::List<String^>();
	System::Xml::Linq::XElement^ retValue = gcnew System::Xml::Linq::XElement("StructureExtents");
	if(pUnmanagedRNA->segments.size() > 0)
	{
		for(int i=0; i<pUnmanagedRNA->segments.size(); i++)
		{
			String^ id = System::String::Format("{0:X}", (int)(pUnmanagedRNA->segments[i].label));
			if(!extentIDs->Contains(id))
			{
				XElement^ extent = nullptr;
				if(pUnmanagedRNA->segments[i].label->type==RNA::Label::HELIX)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() == 2) //If not, there is an error
					{
						extent = gcnew XElement("Helix", gcnew XAttribute("ID", id));
						extent->Add(gcnew XElement("FivePrimeStart", (pUnmanagedRNA->segments[i].label->segments[0]->start+1)));
						extent->Add(gcnew XElement("FivePrimeEnd", (pUnmanagedRNA->segments[i].label->segments[0]->end+1)));
						extent->Add(gcnew XElement("ThreePrimeStart", (pUnmanagedRNA->segments[i].label->segments[1]->start+1)));
						extent->Add(gcnew XElement("ThreePrimeEnd", (pUnmanagedRNA->segments[i].label->segments[1]->end+1)));
					}
				}
				else if(pUnmanagedRNA->segments[i].label->type==RNA::Label::KNOT)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() == 2)
					{
						extent = gcnew XElement("Helix-Knot", gcnew XAttribute("ID", id));
						extent->Add(gcnew XElement("FivePrimeStart", (pUnmanagedRNA->segments[i].label->segments[0]->start+1)));
						extent->Add(gcnew XElement("FivePrimeEnd", (pUnmanagedRNA->segments[i].label->segments[0]->end+1)));
						extent->Add(gcnew XElement("ThreePrimeStart", (pUnmanagedRNA->segments[i].label->segments[1]->start+1)));
						extent->Add(gcnew XElement("ThreePrimeEnd", (pUnmanagedRNA->segments[i].label->segments[1]->end+1)));
					}
				}
				else if(pUnmanagedRNA->segments[i].label->type==RNA::Label::INTERNAL)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() == 2) //Again, if not, there is an error
					{
						extent = gcnew XElement("InternalLoop", gcnew XAttribute("ID", id));
						extent->Add(gcnew XElement("FivePrimeStrandStart", (pUnmanagedRNA->segments[i].label->segments[0]->start+1)));
						extent->Add(gcnew XElement("FivePrimeStrandEnd", (pUnmanagedRNA->segments[i].label->segments[0]->end+1)));
						extent->Add(gcnew XElement("ThreePrimeStrandStart", (pUnmanagedRNA->segments[i].label->segments[1]->start+1)));
						extent->Add(gcnew XElement("ThreePrimeStrandEnd", (pUnmanagedRNA->segments[i].label->segments[1]->end+1)));
					}
				}
				else if(pUnmanagedRNA->segments[i].label->type==RNA::Label::BULGE)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() == 1)
					{
						extent = gcnew XElement("BulgeLoop", gcnew XAttribute("ID", id));
						extent->Add(gcnew XElement("StrandStart", (pUnmanagedRNA->segments[i].label->segments[0]->start+1)));
						extent->Add(gcnew XElement("StrandEnd", (pUnmanagedRNA->segments[i].label->segments[0]->end+1)));
					}
				}
				else if(pUnmanagedRNA->segments[i].label->type==RNA::Label::MULTISTEM)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() > 0)
					{
						extent = gcnew XElement("MultistemLoop", gcnew XAttribute("ID", id));
						for(int k=0; k<pUnmanagedRNA->segments[i].label->segments.size(); k++)
						{
							XElement^ stemstrand = gcnew XElement("Strand");
							stemstrand->Add(gcnew XElement("StrandStart", (pUnmanagedRNA->segments[i].label->segments[k]->start+1)));
							stemstrand->Add(gcnew XElement("StrandEnd", (pUnmanagedRNA->segments[i].label->segments[k]->end+1)));
							extent->Add(stemstrand);
						}
					}
				}
				else if(pUnmanagedRNA->segments[i].label->type==RNA::Label::HAIRPIN)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() == 1)
					{
						extent = gcnew XElement("HairpinLoop", gcnew XAttribute("ID", id));
						extent->Add(gcnew XElement("StrandStart", (pUnmanagedRNA->segments[i].label->segments[0]->start+1)));
						extent->Add(gcnew XElement("StrandEnd", (pUnmanagedRNA->segments[i].label->segments[0]->end+1)));
					}
				}
				else if(pUnmanagedRNA->segments[i].label->type==RNA::Label::FREE)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() == 1)
					{
						extent = gcnew XElement("Free", gcnew XAttribute("ID", id));
						extent->Add(gcnew XElement("StrandStart", (pUnmanagedRNA->segments[i].label->segments[0]->start+1)));
						extent->Add(gcnew XElement("StrandEnd", (pUnmanagedRNA->segments[i].label->segments[0]->end+1)));
					}
				}
				else if(pUnmanagedRNA->segments[i].label->type==RNA::Label::TAIL)
				{
					if(pUnmanagedRNA->segments[i].label->segments.size() == 1)
					{
						extent = gcnew XElement("Tail", gcnew XAttribute("ID", id));
						extent->Add(gcnew XElement("StrandStart", (pUnmanagedRNA->segments[i].label->segments[0]->start+1)));
						extent->Add(gcnew XElement("StrandEnd", (pUnmanagedRNA->segments[i].label->segments[0]->end+1)));
					}
				}

				if(extent!=nullptr)
				{
					if(pUnmanagedRNA->segments[i].label->touches.size()>0)
					{
						for(int j=0; j<pUnmanagedRNA->segments[i].label->touches.size(); j++)
						{
							System::String^ adjacentid = System::String::Format("{0:X}", (int)pUnmanagedRNA->segments[i].label->touches[j]);
							XElement^ adjacentextent = gcnew XElement("AdjacentExtent", gcnew XAttribute("ID", adjacentid));
							extent->Add(adjacentextent);
						}
					}
					retValue->Add(extent);
					extentIDs->Add(id);
				}
			}
		}
	}
	return retValue;
}

System::Collections::Generic::Dictionary<System::String^, System::String^>^ ManagedLabelRNA::ManagedRNA::GetStructureElements()
{
	System::Collections::Generic::Dictionary<System::String^, System::String^>^ retValue = gcnew System::Collections::Generic::Dictionary<System::String^, System::String^>();
	if(pUnmanagedRNA->segments.size() > 0)
	{
		for(int i=0; i<pUnmanagedRNA->segments.size(); i++)
		{
			String^ id = System::String::Format("{0:X}", (int)(pUnmanagedRNA->segments[i].label));
			if(!retValue->ContainsKey(id))
			{
				//Element Format: id,type,parent,touches1id::touches2id::...,segments
				System::String^ element = id + "," + ConvertTypeToString(pUnmanagedRNA->segments[i].label->type) + ",";
				if(pUnmanagedRNA->segments[i].label->parent!=nullptr)
				{
					String^ parentid = System::String::Format("{0:X}", (int)pUnmanagedRNA->segments[i].label->parent);
					element += parentid;
				}
				
				element += ",";

				String^ touchedelements = nullptr;
				if(pUnmanagedRNA->segments[i].label->touches.size() > 0)
				{
					for(int k=0; k<pUnmanagedRNA->segments[i].label->touches.size(); k++)
					{
						touchedelements += System::String::Format("{0:X}", (int)pUnmanagedRNA->segments[i].label->touches[k]);
						if(k<pUnmanagedRNA->segments[i].label->touches.size()-1) touchedelements += "::";
					}
					element += touchedelements;
				}

				element += ",";

				if(pUnmanagedRNA->segments[i].label->segments.size() > 0)
				{
					for(int l=0; l<pUnmanagedRNA->segments[i].label->segments.size(); l++)
					{
						element += ((pUnmanagedRNA->segments[i].label->segments[l]->start)+1) + "," + ((pUnmanagedRNA->segments[i].label->segments[l]->end)+1);
						if(l<pUnmanagedRNA->segments[i].label->segments.size()-1) element += ",";
					}
				}

				retValue->Add(id, element);
			}
		}
	}
	return retValue;
}

System::String^ ManagedLabelRNA::ManagedRNA::ConvertTypeToString(RNA::Label::Type type)
{
	System::String^ retValue;
	switch(type)
	{
	case RNA::Label::HELIX:
		retValue = "HELIX";
		break;
	case RNA::Label::BULGE:
		retValue = "BULGE";
		break;
	case RNA::Label::KNOT:
		retValue = "HELIX-KNOT";
		break;
	case RNA::Label::HAIRPIN:
		retValue = "HAIRPIN";
		break;
	case RNA::Label::INTERNAL:
		retValue = "INTERNAL";
		break;
	case RNA::Label::FREE:
		retValue = "FREE";
		break;
	case RNA::Label::MULTISTEM:
		retValue = "MULTISTEM";
		break;
	case RNA::Label::TAIL:
		retValue = "TAIL";
		break;
	default:
		retValue = "";
		break;
	};
	return retValue;
}
