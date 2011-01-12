/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#pragma once

#include "Enums.h"

namespace SlimDX
{
	namespace DirectInput
	{
		public ref class DeviceImage
		{
		private:
			System::String^ path;
			ImageUsage usage;
			int viewId;
			System::Drawing::Rectangle overlay;
			int instance;
			ObjectDeviceType type;
			array<System::Drawing::Point>^ calloutLine;
			System::Drawing::Rectangle calloutRectangle;
			TextAlignment alignment;

		internal:
			DeviceImage( const DIDEVICEIMAGEINFO &image );

		public:
			property System::String^ Path
			{
				System::String^ get() { return path; }
			}

			property ImageUsage Usage
			{
				ImageUsage get() { return usage; }
			}

			property int ViewId
			{
				int get() { return viewId; }
			}

			property System::Drawing::Rectangle Overlay
			{
				System::Drawing::Rectangle get() { return overlay; }
			}
			
			property int Instance
			{
				int get() { return instance; }
			}

			property ObjectDeviceType Type
			{
				ObjectDeviceType get() { return type; }
			}

			property array<System::Drawing::Point>^ CalloutLine
			{
				array<System::Drawing::Point>^ get() { return calloutLine; }
			}

			property System::Drawing::Rectangle CalloutRectangle
			{
				System::Drawing::Rectangle get() { return calloutRectangle; }
			}

			property TextAlignment Alignment
			{
				TextAlignment get() { return alignment; }
			}
		};
	}
}