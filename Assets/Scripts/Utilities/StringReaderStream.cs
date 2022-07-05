/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        References : 
        If you use this code, please cite the following reference :         
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990
       
        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/


using System;
using System.IO;
using System.Text;

public class StringReaderStream : Stream
{
	private string input;
	private readonly Encoding encoding;
	private int maxBytesPerChar;
	private int inputLength;
	private int inputPosition;
	private readonly long length;
	private long position;

	public StringReaderStream(string input)
		: this(input, Encoding.UTF8)
	{ }

	public StringReaderStream(string input, Encoding encoding)
	{
		this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
		this.input = input;
		inputLength = input == null ? 0 : input.Length;
		if (!string.IsNullOrEmpty(input))
			length = encoding.GetByteCount(input);
		maxBytesPerChar = encoding == Encoding.ASCII ? 1 : encoding.GetMaxByteCount(1);
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => length;

	public override long Position
	{
		get => position;
		set => throw new NotImplementedException();
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (inputPosition >= inputLength)
			return 0;
		if (count < maxBytesPerChar)
			throw new ArgumentException("count has to be greater or equal to max encoding byte count per char");
		int charCount = Math.Min(inputLength - inputPosition, count / maxBytesPerChar);
		int byteCount = encoding.GetBytes(input, inputPosition, charCount, buffer, offset);
		inputPosition += charCount;
		position += byteCount;
		return byteCount;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
