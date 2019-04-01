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

#ifndef _RNA_H_
#define _RNA_H_

#include "Array.h"
#include "exceptions.h"

class RNA {
public:
	typedef int Binding;
	static const Binding UNBOUND;

	struct Label;
	struct Segment {
		Binding start, end;
		Label* label;
	};
	struct Label {
		enum Type {HELIX, KNOT, FREE, TAIL, BULGE, HAIRPIN, INTERNAL, MULTISTEM} type;
		// FREE, TAIL, BULGE, HAIRPIN use segments[0]
		// HELIX, INTERNAL use segments[0-1]
		// MULTISTEM uses segments[0-2+]
		MyArray::Array<Segment*> segments;
		// Used by knots to record crossing knots
		// Used by loops to record the helices they touch
		MyArray::Array<Label*> touches;
		// Points to the label preceding the 5' end of this label
		Label* parent;
		// False if pseudoknot requires further processing
		bool resolved;

		bool isHelix() const {
			return type == HELIX || type == KNOT;
		}
		bool isLoop() const {
			return !isHelix();
		}
	};
	
	RNA();
	RNA(const RNA&) {assert(false);}
	/*RNA(const char* filename) throw(io_error);

	bool load(const char* filename);*/
	bool computeTiered();

#ifdef USE_PLOTTER
	// Produces an EPS image
	bool plot(const char* filename) const;
#endif

//private:
	// multistems with no knots
	bool traceMultistem1(Label* start, Binding pos);
	// multistems that skip over knots
	bool traceMultistem2(Label* start, Binding pos);
	// multistems that bridge knots
	bool traceMultistem3(Label* start, Binding pos);
	enum HelixRel {AINB, BINA, DISJOINT, CROSS};
	static HelixRel cmp(const Label& a, const Label& b);

	// Tier 1
	//MyArray::Array<char> sequence;
	MyArray::Array<Binding> bindings;
	// Tier 2
	MyArray::Array<Segment> segments;
	MyArray::Array<Segment*> bindingsToSegments;
	// Tier 3
	MyArray::Array<Label> labels;
};

#endif
