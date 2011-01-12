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

#include "DeviceDI.h"

namespace SlimDX
{
	namespace DirectInput
	{
		generic<typename TDataFormat>
		public ref class CustomDevice : public Device
		{
			COMOBJECT_CUSTOM(IDirectInputDevice8, CustomDevice);

		public:
			/// <summary>
			/// Initializes a new instance of the <see cref="Device"/> class.
			/// </summary>
			/// <param name="subsystem">The subsystem identifier.</param>
			[System::Security::Permissions::SecurityPermission( System::Security::Permissions::SecurityAction::LinkDemand, Flags=System::Security::Permissions::SecurityPermissionFlag::UnmanagedCode )]
			CustomDevice( DirectInput^ directInput, System::Guid subsystem );

			/// <summary>
			/// Retrieves the current device state.
			/// </summary>
			/// <returns>The current device state.</returns>
			TDataFormat GetCurrentState();

			/// <summary>
			/// Retrieves the current device state.
			/// </summary>
			/// <returns>A <see cref="SlimDX::Result"/> object describing the result of the operation.</returns>
			[System::Security::Permissions::SecurityPermission( System::Security::Permissions::SecurityAction::LinkDemand, Flags=System::Security::Permissions::SecurityPermissionFlag::UnmanagedCode )]
			Result GetCurrentState( TDataFormat% data );

			/// <summary>
			/// Retrieves buffered data from the device.
			/// </summary>
			/// <returns>A collection of buffered input events.</returns>
			System::Collections::Generic::IList<TDataFormat>^ GetBufferedData();

			/// <summary>
			/// Gets properties about a single object on an input device.
			/// </summary>
			/// <param name="name">The name of the object whose properties are to be retrieved.</param>
			/// <returns>The properties of the desired object.</returns>
			ObjectProperties^ GetObjectPropertiesByName( System::String^ name );

			DeviceObjectInstance GetObjectInfoByName( System::String^ name );
		};
	}
}