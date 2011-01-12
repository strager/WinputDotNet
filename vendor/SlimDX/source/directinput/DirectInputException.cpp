#include "stdafx.h"
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

#include "DirectInputException.h"

using namespace System;
using namespace System::Runtime::Serialization;

namespace SlimDX
{
namespace DirectInput
{
	DirectInputException::DirectInputException( SerializationInfo^ info, StreamingContext context )
	: SlimDXException( info, context )
	{
	}

	DirectInputException::DirectInputException()
	{
	}

	DirectInputException::DirectInputException( String^ message )
	: SlimDXException( message )
	{
	}

	DirectInputException::DirectInputException( String^ message, Exception^ innerException )
	: SlimDXException( message, innerException )
	{
	}

	DirectInputException::DirectInputException( Result result )
	: SlimDXException( result )
	{
	}

	Result RecordError( HRESULT hr )
	{
		if( hr == DIERR_OTHERAPPHASPRIO || hr == DIERR_INPUTLOST )
			return Result::Record<DirectInputException^>( hr, false, nullptr, nullptr );
		else
			return RECORD_DINPUT( hr );
	}
}
}
