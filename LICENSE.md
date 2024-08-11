# License

[Plot](https://github.com/cecarlsen/dk.cec.plot), an immediate mode (IM) procedural 2D drawing package for Unity, is released under the standard MIT license – however, it depends on the following third party code with other licenses:

- [Earcut.net](https://github.com/oberbichler/earcut.net) by Thomas Oberbichler, for triangulation (ISC license).
- [Chroma.js](https://github.com/gka/chroma.js) by Gregor Aisch, for JCh color conversion (BSD license).

Plot license:
	
	MIT License
	
	Copyright (c) 2020-2024 Carl Emil Carlsen (https://cec.dk)
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.



## Third Party Licenses

### Earcut.net

[Earcut.net](https://github.com/oberbichler/earcut.net), used for triangulation in Plot, is a port of earcut.js by Mapbox, both of which are under ISC license. Contained in *dk.cec.plot/Runtime/Internal/EarcutNet/Earcut.cs*.

	ISC License
	
	Copyright (c) 2018, Thomas Oberbichler.
	
	Modified 2020, Carl Emil Carlsen.
	
	Permission to use, copy, modify, and/or distribute this software for any purpose
	with or without fee is hereby granted, provided that the above copyright notice
	and this permission notice appear in all copies.
	
	THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
	REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
	FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
	INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
	OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
	TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
	THIS SOFTWARE.


### Chroma.js

[Chroma.js](https://github.com/gka/chroma.js), specifically the code that relates to jCh color conversion is used in Plot, is under BSD license. Contained in *dk.cec.plot/Runtime/Color/JChColor.cs*.

	BSD License
	
	Copyright (c) 2011-2019, Gregor Aisch
	All rights reserved.
	
	Ported to C# and modified 2020, Carl Emil Carlsen and Jacob Harder.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:

	1. Redistributions of source code must retain the above copyright notice, this
		list of conditions and the following disclaimer.

	2. Redistributions in binary form must reproduce the above copyright notice,
		his list of conditions and the following disclaimer in the documentation
		and/or other materials provided with the distribution.

	3. The name Gregor Aisch may not be used to endorse or promote products
		derived from this software without specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
	AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
	IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL GREGOR AISCH OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
	INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
	BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
	DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
	OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
	NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
	EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.