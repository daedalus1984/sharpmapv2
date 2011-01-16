
/*
 *	Portions of this file are  � 2008 Newgrove Consultants Limited, 
 *  http://www.newgrove.com; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/    
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.  
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: John Diss 2008
 *
 *  Original notices below.
 * 
 */
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
//
// Class gsv_text
//
//----------------------------------------------------------------------------
using System;
using AGG.Transform;
using NPack.Interfaces;
using NPack;

namespace AGG.VertexSource
{
    public sealed class GsvText<T> : IVertexSource<T>
         where T : IEquatable<T>, IComparable<T>, IComputable<T>, IConvertible, IFormattable, ICommonNumericalOperations<T>, ITrigonometricOperations<T>
    {
        enum Status
        {
            Initial,
            NextChar,
            StartGlyph,
            Glyph
        };

        T m_StartX;
        T m_CurrentX;
        T m_CurrentY;
        T m_WidthRatioOfHeight;
        T m_FontSize;
        T m_SpaceBetweenCharacters;
        T m_SpaceBetweenLines;
        string m_Text;
        int m_CurrentCharacterIndex;
        byte[] m_font;
        Status m_status;
        bool m_big_endian;
        int m_StartOfIndicesIndex;
        int m_StartOfGlyphsIndex;
        int m_BeginGlyphIndex;
        int m_EndGlyphIndex;
        T m_WidthScaleRatio;
        T m_HeightScaleRatio;

        public T AscenderHeight
        {
            get
            {
                return m_FontSize.Multiply(.15);
            }
        }
        public T DescenderHeight
        {
            get
            {
                return m_FontSize.Multiply(.2);
            }
        }

        public GsvText()
        {
            m_CurrentX = M.Zero<T>();
            m_CurrentY = M.Zero<T>();
            m_StartX = M.Zero<T>();
            m_WidthRatioOfHeight = M.One<T>();
            m_FontSize = M.Zero<T>();
            m_SpaceBetweenCharacters = M.Zero<T>();
            m_font = CGSVDefaultFont.GsvDefaultFont;
            m_status = Status.Initial;
            m_big_endian = false;

            m_SpaceBetweenLines = M.Zero<T>();

            int t = 1;
            unsafe
            {
                if (*(byte*)&t == 0) m_big_endian = true;
            }
        }

        /*
        public void font(void* font)
        {
            m_font = font;
            if(m_font == 0) m_font = &m_loaded_font[0];
        }
         */

        public void LoadFont(string file)
        {
            throw new System.NotImplementedException();
            /*
            m_loaded_font.resize(0);
            FILE* fd = fopen(file, "rb");
            if(fd)
            {
                uint len;

                fseek(fd, 0l, SEEK_END);
                len = ftell(fd);
                fseek(fd, 0l, SEEK_SET);
                if(len > 0)
                {
                    m_loaded_font.resize(len);
                    fread(&m_loaded_font[0], 1, len, fd);
                    m_font = &m_loaded_font[0];
                }
                fclose(fd);
            }
             */
        }

        // This will set the desired height.  NOTE: The font may not render at the size that you say.
        // It depends on the way the font was originally created.  A 24 Point font may not actually be 24 points high
        public void SetFontSize(T fontSize)
        {
            SetFontSizeAndWidthRatio(fontSize, M.One<T>());
        }

        public void SetFontSize(double fontSize)
        {
            SetFontSize(M.New<T>(fontSize));
        }

        public void SetFontSizeAndWidthRatio(T fontSize, T widthRatioOfHeight)
        {
            if (fontSize.Equals(0) || widthRatioOfHeight.Equals(0))
            {
                throw new System.Exception("You can't have a font with 0 width or height.  Nothing will render.");
            }

            m_FontSize = fontSize;
            m_WidthRatioOfHeight = widthRatioOfHeight;

            m_SpaceBetweenLines = m_FontSize.Multiply(1.5);
        }

        public void SetSpaceBetweenCharacters(T spaceBetweenCharacters)
        {
            m_SpaceBetweenCharacters = spaceBetweenCharacters;
        }

        public void LineSpace(T spaceBetweenLines)
        {
            m_SpaceBetweenLines = spaceBetweenLines;
        }

        public void StartPoint(double x, double y)
        {
            StartPoint(M.New<T>(x), M.New<T>(y));
        }

        public void StartPoint(T x, T y)
        {
            m_CurrentX = m_StartX = x;
            m_CurrentY = y;
        }

        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                m_Text = value;
            }
        }

        //public void text(string text)
        //{
        //    m_Text = text;
        //}

        private ushort Value(int indicesIndex)
        {
            ushort v;
            if (m_big_endian)
            {
                v = (ushort)(m_font[indicesIndex + 0] << 8);
                v |= m_font[indicesIndex + 1];
            }
            else
            {
                v = m_font[indicesIndex + 0];
                v |= (ushort)(m_font[indicesIndex + 1] << 8);
            }
            return v;
        }

        public void Rewind(uint nothing)
        {
            m_status = Status.Initial;
            if (m_font == null) return;

            m_StartOfIndicesIndex = Value(0);

            double base_height = Value(4);
            m_StartOfGlyphsIndex = m_StartOfIndicesIndex + 257 * 2; // one for x one for y
            m_HeightScaleRatio = m_FontSize.Divide(base_height);
            m_WidthScaleRatio = m_HeightScaleRatio.Multiply(m_WidthRatioOfHeight);
            m_CurrentCharacterIndex = 0;
        }

        private void GetSize(char characterToMeasure, out T width, out T height)
        {
            width = M.Zero<T>();
            height = M.Zero<T>();
            if (m_font == null)
            {
                return;
            }

            int maskedChracter = (int)(characterToMeasure & 0xFF);
            if (maskedChracter == '\r' || maskedChracter == '\n')
            {
                height.SubtractEquals(m_FontSize.Add(m_SpaceBetweenLines));
                return;
            }

            int maskedChracterGlyphIndex = maskedChracter * 2; // we have an x and y in the array so it's * 2.
            int BeginGlyphIndex = m_StartOfGlyphsIndex + Value(m_StartOfIndicesIndex + maskedChracterGlyphIndex);
            int EndGlyphIndex = m_StartOfGlyphsIndex + Value(m_StartOfIndicesIndex + maskedChracterGlyphIndex + 2);

            do
            {
                if (BeginGlyphIndex >= EndGlyphIndex)
                {
                    return; // the character has no glyph
                }

                unsafe
                {
                    unchecked
                    {
                        fixed (byte* pFont = m_font)
                        {
                            sbyte* pFontSByte = (sbyte*)pFont;
                            int DeltaX = (int)(pFontSByte[BeginGlyphIndex++]);
                            sbyte yc = pFontSByte[BeginGlyphIndex++];
                            yc <<= 1;
                            yc >>= 1;
                            int DeltaY = (int)(yc);
                            width.AddEquals(M.New<T>(DeltaX).Multiply(m_WidthScaleRatio));
                            height.AddEquals(M.New<T>(DeltaY).Multiply(m_HeightScaleRatio));
                        }
                    }
                }
            } while (true);
        }

        public void GetSize(int characterToMeasureStartIndexInclusive, int characterToMeasureEndIndexInclusive,
            out IVector<T> offset)
        {
            //offset =  MatrixFactory<T>.CreateVector2D ();
            //offset.x = 0;
            //offset.y = 0;
            T x = M.Zero<T>(), y = M.Zero<T>();
            if (m_Text.Length > 0)
            {
                characterToMeasureStartIndexInclusive = Math.Max(0, Math.Min(characterToMeasureStartIndexInclusive, m_Text.Length - 1));
                characterToMeasureEndIndexInclusive = Math.Max(0, Math.Min(characterToMeasureEndIndexInclusive, m_Text.Length - 1));
                for (int i = characterToMeasureStartIndexInclusive; i <= characterToMeasureEndIndexInclusive; i++)
                {
                    char singleChar = m_Text[i];
                    if (singleChar == '\r' || singleChar == '\n')
                    {
                        x = M.Zero<T>();
                        y.SubtractEquals(m_FontSize.Add(m_SpaceBetweenLines));
                    }
                    else
                    {
                        T sigleWidth;
                        T sigleHeight;
                        GetSize(singleChar, out sigleWidth, out sigleHeight);
                        x.AddEquals(sigleWidth.Add(m_SpaceBetweenCharacters));
                        y.AddEquals(sigleHeight);
                    }
                }
            }
            offset = MatrixFactory<T>.CreateVector2D(x, y);
        }

        public uint Vertex(out T x, out T y)
        {
            x = M.Zero<T>();
            y = M.Zero<T>();
            bool quit = false;

            while (!quit)
            {
                switch (m_status)
                {
                    case Status.Initial:
                        if (m_font == null)
                        {
                            quit = true;
                            break;
                        }
                        m_status = Status.NextChar;
                        goto case Status.NextChar;

                    case Status.NextChar:
                        if (m_CurrentCharacterIndex == m_Text.Length)
                        {
                            quit = true;
                            break;
                        }
                        int maskedChracter = (int)((m_Text[m_CurrentCharacterIndex++]) & 0xFF);
                        if (maskedChracter == '\r' || maskedChracter == '\n')
                        {
                            m_CurrentX = m_StartX;
                            m_CurrentY.SubtractEquals(m_FontSize.Add(m_SpaceBetweenLines));
                            break;
                        }
                        int maskedChracterGlyphIndex = maskedChracter * 2; // we have an x and y in the array so it's * 2.
                        m_BeginGlyphIndex = m_StartOfGlyphsIndex + Value(m_StartOfIndicesIndex + maskedChracterGlyphIndex);
                        m_EndGlyphIndex = m_StartOfGlyphsIndex + Value(m_StartOfIndicesIndex + maskedChracterGlyphIndex + 2);
                        m_status = Status.StartGlyph;
                        goto case Status.StartGlyph;

                    case Status.StartGlyph:
                        x = m_CurrentX;
                        y = m_CurrentY;
                        m_status = Status.Glyph;
                        return (uint)Path.Commands.MoveTo;

                    case Status.Glyph:
                        if (m_BeginGlyphIndex >= m_EndGlyphIndex)
                        {
                            m_status = Status.NextChar;
                            m_CurrentX.AddEquals(m_SpaceBetweenCharacters);
                            break;
                        }

                        sbyte IsAMoveTo_Flag;
                        unsafe
                        {
                            unchecked
                            {
                                sbyte yc;
                                fixed (byte* pFont = m_font)
                                {
                                    sbyte* pFontSByte = (sbyte*)pFont;
                                    int DeltaX = (int)(pFontSByte[m_BeginGlyphIndex++]);
                                    IsAMoveTo_Flag = (sbyte)((yc = pFontSByte[m_BeginGlyphIndex++]) & 0x80);
                                    yc <<= 1;
                                    yc >>= 1;
                                    int DeltaY = (int)(yc);
                                    m_CurrentX.AddEquals(M.New<T>(DeltaX).Multiply(m_WidthScaleRatio));
                                    m_CurrentY.AddEquals(M.New<T>(DeltaY).Multiply(m_HeightScaleRatio));
                                }
                            }
                        }
                        x = m_CurrentX;
                        y = m_CurrentY;
                        if (IsAMoveTo_Flag != 0)
                        {
                            return (uint)Path.Commands.MoveTo;
                        }

                        return (uint)Path.Commands.LineTo;

                    default:
                        throw new System.Exception("Unknown Status");
                }
            }

            return (uint)Path.Commands.Stop;
        }
    };

    internal static class CGSVDefaultFont
    {
        static public byte[] GsvDefaultFont = 
		{
			0x40,0x00,0x6c,0x0f,0x15,0x00,0x0e,0x00,0xf9,0xff,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x0d,0x0a,0x0d,0x0a,0x46,0x6f,0x6e,0x74,0x20,0x28,
			0x63,0x29,0x20,0x4d,0x69,0x63,0x72,0x6f,0x50,0x72,
			0x6f,0x66,0x20,0x32,0x37,0x20,0x53,0x65,0x70,0x74,
			0x65,0x6d,0x62,0x2e,0x31,0x39,0x38,0x39,0x00,0x0d,
			0x0a,0x0d,0x0a,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
			0x02,0x00,0x12,0x00,0x34,0x00,0x46,0x00,0x94,0x00,
			0xd0,0x00,0x2e,0x01,0x3e,0x01,0x64,0x01,0x8a,0x01,
			0x98,0x01,0xa2,0x01,0xb4,0x01,0xba,0x01,0xc6,0x01,
			0xcc,0x01,0xf0,0x01,0xfa,0x01,0x18,0x02,0x38,0x02,
			0x44,0x02,0x68,0x02,0x98,0x02,0xa2,0x02,0xde,0x02,
			0x0e,0x03,0x24,0x03,0x40,0x03,0x48,0x03,0x52,0x03,
			0x5a,0x03,0x82,0x03,0xec,0x03,0xfa,0x03,0x26,0x04,
			0x4c,0x04,0x6a,0x04,0x7c,0x04,0x8a,0x04,0xb6,0x04,
			0xc4,0x04,0xca,0x04,0xe0,0x04,0xee,0x04,0xf8,0x04,
			0x0a,0x05,0x18,0x05,0x44,0x05,0x5e,0x05,0x8e,0x05,
			0xac,0x05,0xd6,0x05,0xe0,0x05,0xf6,0x05,0x00,0x06,
			0x12,0x06,0x1c,0x06,0x28,0x06,0x36,0x06,0x48,0x06,
			0x4e,0x06,0x60,0x06,0x6e,0x06,0x74,0x06,0x84,0x06,
			0xa6,0x06,0xc8,0x06,0xe6,0x06,0x08,0x07,0x2c,0x07,
			0x3c,0x07,0x68,0x07,0x7c,0x07,0x8c,0x07,0xa2,0x07,
			0xb0,0x07,0xb6,0x07,0xd8,0x07,0xec,0x07,0x10,0x08,
			0x32,0x08,0x54,0x08,0x64,0x08,0x88,0x08,0x98,0x08,
			0xac,0x08,0xb6,0x08,0xc8,0x08,0xd2,0x08,0xe4,0x08,
			0xf2,0x08,0x3e,0x09,0x48,0x09,0x94,0x09,0xc2,0x09,
			0xc4,0x09,0xd0,0x09,0xe2,0x09,0x04,0x0a,0x0e,0x0a,
			0x26,0x0a,0x34,0x0a,0x4a,0x0a,0x66,0x0a,0x70,0x0a,
			0x7e,0x0a,0x8e,0x0a,0x9a,0x0a,0xa6,0x0a,0xb4,0x0a,
			0xd8,0x0a,0xe2,0x0a,0xf6,0x0a,0x18,0x0b,0x22,0x0b,
			0x32,0x0b,0x56,0x0b,0x60,0x0b,0x6e,0x0b,0x7c,0x0b,
			0x8a,0x0b,0x9c,0x0b,0x9e,0x0b,0xb2,0x0b,0xc2,0x0b,
			0xd8,0x0b,0xf4,0x0b,0x08,0x0c,0x30,0x0c,0x56,0x0c,
			0x72,0x0c,0x90,0x0c,0xb2,0x0c,0xce,0x0c,0xe2,0x0c,
			0xfe,0x0c,0x10,0x0d,0x26,0x0d,0x36,0x0d,0x42,0x0d,
			0x4e,0x0d,0x5c,0x0d,0x78,0x0d,0x8c,0x0d,0x8e,0x0d,
			0x90,0x0d,0x92,0x0d,0x94,0x0d,0x96,0x0d,0x98,0x0d,
			0x9a,0x0d,0x9c,0x0d,0x9e,0x0d,0xa0,0x0d,0xa2,0x0d,
			0xa4,0x0d,0xa6,0x0d,0xa8,0x0d,0xaa,0x0d,0xac,0x0d,
			0xae,0x0d,0xb0,0x0d,0xb2,0x0d,0xb4,0x0d,0xb6,0x0d,
			0xb8,0x0d,0xba,0x0d,0xbc,0x0d,0xbe,0x0d,0xc0,0x0d,
			0xc2,0x0d,0xc4,0x0d,0xc6,0x0d,0xc8,0x0d,0xca,0x0d,
			0xcc,0x0d,0xce,0x0d,0xd0,0x0d,0xd2,0x0d,0xd4,0x0d,
			0xd6,0x0d,0xd8,0x0d,0xda,0x0d,0xdc,0x0d,0xde,0x0d,
			0xe0,0x0d,0xe2,0x0d,0xe4,0x0d,0xe6,0x0d,0xe8,0x0d,
			0xea,0x0d,0xec,0x0d,0x0c,0x0e,0x26,0x0e,0x48,0x0e,
			0x64,0x0e,0x88,0x0e,0x92,0x0e,0xa6,0x0e,0xb4,0x0e,
			0xd0,0x0e,0xee,0x0e,0x02,0x0f,0x16,0x0f,0x26,0x0f,
			0x3c,0x0f,0x58,0x0f,0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,
			0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,
			0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,
			0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,0x6c,0x0f,0x10,0x80,
			0x05,0x95,0x00,0x72,0x00,0xfb,0xff,0x7f,0x01,0x7f,
			0x01,0x01,0xff,0x01,0x05,0xfe,0x05,0x95,0xff,0x7f,
			0x00,0x7a,0x01,0x86,0xff,0x7a,0x01,0x87,0x01,0x7f,
			0xfe,0x7a,0x0a,0x87,0xff,0x7f,0x00,0x7a,0x01,0x86,
			0xff,0x7a,0x01,0x87,0x01,0x7f,0xfe,0x7a,0x05,0xf2,
			0x0b,0x95,0xf9,0x64,0x0d,0x9c,0xf9,0x64,0xfa,0x91,
			0x0e,0x00,0xf1,0xfa,0x0e,0x00,0x04,0xfc,0x08,0x99,
			0x00,0x63,0x04,0x9d,0x00,0x63,0x04,0x96,0xff,0x7f,
			0x01,0x7f,0x01,0x01,0x00,0x01,0xfe,0x02,0xfd,0x01,
			0xfc,0x00,0xfd,0x7f,0xfe,0x7e,0x00,0x7e,0x01,0x7e,
			0x01,0x7f,0x02,0x7f,0x06,0x7e,0x02,0x7f,0x02,0x7e,
			0xf2,0x89,0x02,0x7e,0x02,0x7f,0x06,0x7e,0x02,0x7f,
			0x01,0x7f,0x01,0x7e,0x00,0x7c,0xfe,0x7e,0xfd,0x7f,
			0xfc,0x00,0xfd,0x01,0xfe,0x02,0x00,0x01,0x01,0x01,
			0x01,0x7f,0xff,0x7f,0x10,0xfd,0x15,0x95,0xee,0x6b,
			0x05,0x95,0x02,0x7e,0x00,0x7e,0xff,0x7e,0xfe,0x7f,
			0xfe,0x00,0xfe,0x02,0x00,0x02,0x01,0x02,0x02,0x01,
			0x02,0x00,0x02,0x7f,0x03,0x7f,0x03,0x00,0x03,0x01,
			0x02,0x01,0xfc,0xf2,0xfe,0x7f,0xff,0x7e,0x00,0x7e,
			0x02,0x7e,0x02,0x00,0x02,0x01,0x01,0x02,0x00,0x02,
			0xfe,0x02,0xfe,0x00,0x07,0xf9,0x15,0x8d,0xff,0x7f,
			0x01,0x7f,0x01,0x01,0x00,0x01,0xff,0x01,0xff,0x00,
			0xff,0x7f,0xff,0x7e,0xfe,0x7b,0xfe,0x7d,0xfe,0x7e,
			0xfe,0x7f,0xfd,0x00,0xfd,0x01,0xff,0x02,0x00,0x03,
			0x01,0x02,0x06,0x04,0x02,0x02,0x01,0x02,0x00,0x02,
			0xff,0x02,0xfe,0x01,0xfe,0x7f,0xff,0x7e,0x00,0x7e,
			0x01,0x7d,0x02,0x7d,0x05,0x79,0x02,0x7e,0x03,0x7f,
			0x01,0x00,0x01,0x01,0x00,0x01,0xf1,0xfe,0xfe,0x01,
			0xff,0x02,0x00,0x03,0x01,0x02,0x02,0x02,0x00,0x86,
			0x01,0x7e,0x08,0x75,0x02,0x7e,0x02,0x7f,0x05,0x80,
			0x05,0x93,0xff,0x01,0x01,0x01,0x01,0x7f,0x00,0x7e,
			0xff,0x7e,0xff,0x7f,0x06,0xf1,0x0b,0x99,0xfe,0x7e,
			0xfe,0x7d,0xfe,0x7c,0xff,0x7b,0x00,0x7c,0x01,0x7b,
			0x02,0x7c,0x02,0x7d,0x02,0x7e,0xfe,0x9e,0xfe,0x7c,
			0xff,0x7d,0xff,0x7b,0x00,0x7c,0x01,0x7b,0x01,0x7d,
			0x02,0x7c,0x05,0x85,0x03,0x99,0x02,0x7e,0x02,0x7d,
			0x02,0x7c,0x01,0x7b,0x00,0x7c,0xff,0x7b,0xfe,0x7c,
			0xfe,0x7d,0xfe,0x7e,0x02,0x9e,0x02,0x7c,0x01,0x7d,
			0x01,0x7b,0x00,0x7c,0xff,0x7b,0xff,0x7d,0xfe,0x7c,
			0x09,0x85,0x08,0x95,0x00,0x74,0xfb,0x89,0x0a,0x7a,
			0x00,0x86,0xf6,0x7a,0x0d,0xf4,0x0d,0x92,0x00,0x6e,
			0xf7,0x89,0x12,0x00,0x04,0xf7,0x06,0x81,0xff,0x7f,
			0xff,0x01,0x01,0x01,0x01,0x7f,0x00,0x7e,0xff,0x7e,
			0xff,0x7f,0x06,0x84,0x04,0x89,0x12,0x00,0x04,0xf7,
			0x05,0x82,0xff,0x7f,0x01,0x7f,0x01,0x01,0xff,0x01,
			0x05,0xfe,0x00,0xfd,0x0e,0x18,0x00,0xeb,0x09,0x95,
			0xfd,0x7f,0xfe,0x7d,0xff,0x7b,0x00,0x7d,0x01,0x7b,
			0x02,0x7d,0x03,0x7f,0x02,0x00,0x03,0x01,0x02,0x03,
			0x01,0x05,0x00,0x03,0xff,0x05,0xfe,0x03,0xfd,0x01,
			0xfe,0x00,0x0b,0xeb,0x06,0x91,0x02,0x01,0x03,0x03,
			0x00,0x6b,0x09,0x80,0x04,0x90,0x00,0x01,0x01,0x02,
			0x01,0x01,0x02,0x01,0x04,0x00,0x02,0x7f,0x01,0x7f,
			0x01,0x7e,0x00,0x7e,0xff,0x7e,0xfe,0x7d,0xf6,0x76,
			0x0e,0x00,0x03,0x80,0x05,0x95,0x0b,0x00,0xfa,0x78,
			0x03,0x00,0x02,0x7f,0x01,0x7f,0x01,0x7d,0x00,0x7e,
			0xff,0x7d,0xfe,0x7e,0xfd,0x7f,0xfd,0x00,0xfd,0x01,
			0xff,0x01,0xff,0x02,0x11,0xfc,0x0d,0x95,0xf6,0x72,
			0x0f,0x00,0xfb,0x8e,0x00,0x6b,0x07,0x80,0x0f,0x95,
			0xf6,0x00,0xff,0x77,0x01,0x01,0x03,0x01,0x03,0x00,
			0x03,0x7f,0x02,0x7e,0x01,0x7d,0x00,0x7e,0xff,0x7d,
			0xfe,0x7e,0xfd,0x7f,0xfd,0x00,0xfd,0x01,0xff,0x01,
			0xff,0x02,0x11,0xfc,0x10,0x92,0xff,0x02,0xfd,0x01,
			0xfe,0x00,0xfd,0x7f,0xfe,0x7d,0xff,0x7b,0x00,0x7b,
			0x01,0x7c,0x02,0x7e,0x03,0x7f,0x01,0x00,0x03,0x01,
			0x02,0x02,0x01,0x03,0x00,0x01,0xff,0x03,0xfe,0x02,
			0xfd,0x01,0xff,0x00,0xfd,0x7f,0xfe,0x7e,0xff,0x7d,
			0x10,0xf9,0x11,0x95,0xf6,0x6b,0xfc,0x95,0x0e,0x00,
			0x03,0xeb,0x08,0x95,0xfd,0x7f,0xff,0x7e,0x00,0x7e,
			0x01,0x7e,0x02,0x7f,0x04,0x7f,0x03,0x7f,0x02,0x7e,
			0x01,0x7e,0x00,0x7d,0xff,0x7e,0xff,0x7f,0xfd,0x7f,
			0xfc,0x00,0xfd,0x01,0xff,0x01,0xff,0x02,0x00,0x03,
			0x01,0x02,0x02,0x02,0x03,0x01,0x04,0x01,0x02,0x01,
			0x01,0x02,0x00,0x02,0xff,0x02,0xfd,0x01,0xfc,0x00,
			0x0c,0xeb,0x10,0x8e,0xff,0x7d,0xfe,0x7e,0xfd,0x7f,
			0xff,0x00,0xfd,0x01,0xfe,0x02,0xff,0x03,0x00,0x01,
			0x01,0x03,0x02,0x02,0x03,0x01,0x01,0x00,0x03,0x7f,
			0x02,0x7e,0x01,0x7c,0x00,0x7b,0xff,0x7b,0xfe,0x7d,
			0xfd,0x7f,0xfe,0x00,0xfd,0x01,0xff,0x02,0x10,0xfd,
			0x05,0x8e,0xff,0x7f,0x01,0x7f,0x01,0x01,0xff,0x01,
			0x00,0xf4,0xff,0x7f,0x01,0x7f,0x01,0x01,0xff,0x01,
			0x05,0xfe,0x05,0x8e,0xff,0x7f,0x01,0x7f,0x01,0x01,
			0xff,0x01,0x01,0xf3,0xff,0x7f,0xff,0x01,0x01,0x01,
			0x01,0x7f,0x00,0x7e,0xff,0x7e,0xff,0x7f,0x06,0x84,
			0x14,0x92,0xf0,0x77,0x10,0x77,0x04,0x80,0x04,0x8c,
			0x12,0x00,0xee,0xfa,0x12,0x00,0x04,0xfa,0x04,0x92,
			0x10,0x77,0xf0,0x77,0x14,0x80,0x03,0x90,0x00,0x01,
			0x01,0x02,0x01,0x01,0x02,0x01,0x04,0x00,0x02,0x7f,
			0x01,0x7f,0x01,0x7e,0x00,0x7e,0xff,0x7e,0xff,0x7f,
			0xfc,0x7e,0x00,0x7d,0x00,0xfb,0xff,0x7f,0x01,0x7f,
			0x01,0x01,0xff,0x01,0x09,0xfe,0x12,0x8d,0xff,0x02,
			0xfe,0x01,0xfd,0x00,0xfe,0x7f,0xff,0x7f,0xff,0x7d,
			0x00,0x7d,0x01,0x7e,0x02,0x7f,0x03,0x00,0x02,0x01,
			0x01,0x02,0xfb,0x88,0xfe,0x7e,0xff,0x7d,0x00,0x7d,
			0x01,0x7e,0x01,0x7f,0x07,0x8b,0xff,0x78,0x00,0x7e,
			0x02,0x7f,0x02,0x00,0x02,0x02,0x01,0x03,0x00,0x02,
			0xff,0x03,0xff,0x02,0xfe,0x02,0xfe,0x01,0xfd,0x01,
			0xfd,0x00,0xfd,0x7f,0xfe,0x7f,0xfe,0x7e,0xff,0x7e,
			0xff,0x7d,0x00,0x7d,0x01,0x7d,0x01,0x7e,0x02,0x7e,
			0x02,0x7f,0x03,0x7f,0x03,0x00,0x03,0x01,0x02,0x01,
			0x01,0x01,0xfe,0x8d,0xff,0x78,0x00,0x7e,0x01,0x7f,
			0x08,0xfb,0x09,0x95,0xf8,0x6b,0x08,0x95,0x08,0x6b,
			0xf3,0x87,0x0a,0x00,0x04,0xf9,0x04,0x95,0x00,0x6b,
			0x00,0x95,0x09,0x00,0x03,0x7f,0x01,0x7f,0x01,0x7e,
			0x00,0x7e,0xff,0x7e,0xff,0x7f,0xfd,0x7f,0xf7,0x80,
			0x09,0x00,0x03,0x7f,0x01,0x7f,0x01,0x7e,0x00,0x7d,
			0xff,0x7e,0xff,0x7f,0xfd,0x7f,0xf7,0x00,0x11,0x80,
			0x12,0x90,0xff,0x02,0xfe,0x02,0xfe,0x01,0xfc,0x00,
			0xfe,0x7f,0xfe,0x7e,0xff,0x7e,0xff,0x7d,0x00,0x7b,
			0x01,0x7d,0x01,0x7e,0x02,0x7e,0x02,0x7f,0x04,0x00,
			0x02,0x01,0x02,0x02,0x01,0x02,0x03,0xfb,0x04,0x95,
			0x00,0x6b,0x00,0x95,0x07,0x00,0x03,0x7f,0x02,0x7e,
			0x01,0x7e,0x01,0x7d,0x00,0x7b,0xff,0x7d,0xff,0x7e,
			0xfe,0x7e,0xfd,0x7f,0xf9,0x00,0x11,0x80,0x04,0x95,
			0x00,0x6b,0x00,0x95,0x0d,0x00,0xf3,0xf6,0x08,0x00,
			0xf8,0xf5,0x0d,0x00,0x02,0x80,0x04,0x95,0x00,0x6b,
			0x00,0x95,0x0d,0x00,0xf3,0xf6,0x08,0x00,0x06,0xf5,
			0x12,0x90,0xff,0x02,0xfe,0x02,0xfe,0x01,0xfc,0x00,
			0xfe,0x7f,0xfe,0x7e,0xff,0x7e,0xff,0x7d,0x00,0x7b,
			0x01,0x7d,0x01,0x7e,0x02,0x7e,0x02,0x7f,0x04,0x00,
			0x02,0x01,0x02,0x02,0x01,0x02,0x00,0x03,0xfb,0x80,
			0x05,0x00,0x03,0xf8,0x04,0x95,0x00,0x6b,0x0e,0x95,
			0x00,0x6b,0xf2,0x8b,0x0e,0x00,0x04,0xf5,0x04,0x95,
			0x00,0x6b,0x04,0x80,0x0c,0x95,0x00,0x70,0xff,0x7d,
			0xff,0x7f,0xfe,0x7f,0xfe,0x00,0xfe,0x01,0xff,0x01,
			0xff,0x03,0x00,0x02,0x0e,0xf9,0x04,0x95,0x00,0x6b,
			0x0e,0x95,0xf2,0x72,0x05,0x85,0x09,0x74,0x03,0x80,
			0x04,0x95,0x00,0x6b,0x00,0x80,0x0c,0x00,0x01,0x80,
			0x04,0x95,0x00,0x6b,0x00,0x95,0x08,0x6b,0x08,0x95,
			0xf8,0x6b,0x08,0x95,0x00,0x6b,0x04,0x80,0x04,0x95,
			0x00,0x6b,0x00,0x95,0x0e,0x6b,0x00,0x95,0x00,0x6b,
			0x04,0x80,0x09,0x95,0xfe,0x7f,0xfe,0x7e,0xff,0x7e,
			0xff,0x7d,0x00,0x7b,0x01,0x7d,0x01,0x7e,0x02,0x7e,
			0x02,0x7f,0x04,0x00,0x02,0x01,0x02,0x02,0x01,0x02,
			0x01,0x03,0x00,0x05,0xff,0x03,0xff,0x02,0xfe,0x02,
			0xfe,0x01,0xfc,0x00,0x0d,0xeb,0x04,0x95,0x00,0x6b,
			0x00,0x95,0x09,0x00,0x03,0x7f,0x01,0x7f,0x01,0x7e,
			0x00,0x7d,0xff,0x7e,0xff,0x7f,0xfd,0x7f,0xf7,0x00,
			0x11,0xf6,0x09,0x95,0xfe,0x7f,0xfe,0x7e,0xff,0x7e,
			0xff,0x7d,0x00,0x7b,0x01,0x7d,0x01,0x7e,0x02,0x7e,
			0x02,0x7f,0x04,0x00,0x02,0x01,0x02,0x02,0x01,0x02,
			0x01,0x03,0x00,0x05,0xff,0x03,0xff,0x02,0xfe,0x02,
			0xfe,0x01,0xfc,0x00,0x03,0xef,0x06,0x7a,0x04,0x82,
			0x04,0x95,0x00,0x6b,0x00,0x95,0x09,0x00,0x03,0x7f,
			0x01,0x7f,0x01,0x7e,0x00,0x7e,0xff,0x7e,0xff,0x7f,
			0xfd,0x7f,0xf7,0x00,0x07,0x80,0x07,0x75,0x03,0x80,
			0x11,0x92,0xfe,0x02,0xfd,0x01,0xfc,0x00,0xfd,0x7f,
			0xfe,0x7e,0x00,0x7e,0x01,0x7e,0x01,0x7f,0x02,0x7f,
			0x06,0x7e,0x02,0x7f,0x01,0x7f,0x01,0x7e,0x00,0x7d,
			0xfe,0x7e,0xfd,0x7f,0xfc,0x00,0xfd,0x01,0xfe,0x02,
			0x11,0xfd,0x08,0x95,0x00,0x6b,0xf9,0x95,0x0e,0x00,
			0x01,0xeb,0x04,0x95,0x00,0x71,0x01,0x7d,0x02,0x7e,
			0x03,0x7f,0x02,0x00,0x03,0x01,0x02,0x02,0x01,0x03,
			0x00,0x0f,0x04,0xeb,0x01,0x95,0x08,0x6b,0x08,0x95,
			0xf8,0x6b,0x09,0x80,0x02,0x95,0x05,0x6b,0x05,0x95,
			0xfb,0x6b,0x05,0x95,0x05,0x6b,0x05,0x95,0xfb,0x6b,
			0x07,0x80,0x03,0x95,0x0e,0x6b,0x00,0x95,0xf2,0x6b,
			0x11,0x80,0x01,0x95,0x08,0x76,0x00,0x75,0x08,0x95,
			0xf8,0x76,0x09,0xf5,0x11,0x95,0xf2,0x6b,0x00,0x95,
			0x0e,0x00,0xf2,0xeb,0x0e,0x00,0x03,0x80,0x03,0x93,
			0x00,0x6c,0x01,0x94,0x00,0x6c,0xff,0x94,0x05,0x00,
			0xfb,0xec,0x05,0x00,0x02,0x81,0x00,0x95,0x0e,0x68,
			0x00,0x83,0x06,0x93,0x00,0x6c,0x01,0x94,0x00,0x6c,
			0xfb,0x94,0x05,0x00,0xfb,0xec,0x05,0x00,0x03,0x81,
			0x03,0x87,0x08,0x05,0x08,0x7b,0xf0,0x80,0x08,0x04,
			0x08,0x7c,0x03,0xf9,0x01,0x80,0x10,0x00,0x01,0x80,
			0x06,0x95,0xff,0x7f,0xff,0x7e,0x00,0x7e,0x01,0x7f,
			0x01,0x01,0xff,0x01,0x05,0xef,0x0f,0x8e,0x00,0x72,
			0x00,0x8b,0xfe,0x02,0xfe,0x01,0xfd,0x00,0xfe,0x7f,
			0xfe,0x7e,0xff,0x7d,0x00,0x7e,0x01,0x7d,0x02,0x7e,
			0x02,0x7f,0x03,0x00,0x02,0x01,0x02,0x02,0x04,0xfd,
			0x04,0x95,0x00,0x6b,0x00,0x8b,0x02,0x02,0x02,0x01,
			0x03,0x00,0x02,0x7f,0x02,0x7e,0x01,0x7d,0x00,0x7e,
			0xff,0x7d,0xfe,0x7e,0xfe,0x7f,0xfd,0x00,0xfe,0x01,
			0xfe,0x02,0x0f,0xfd,0x0f,0x8b,0xfe,0x02,0xfe,0x01,
			0xfd,0x00,0xfe,0x7f,0xfe,0x7e,0xff,0x7d,0x00,0x7e,
			0x01,0x7d,0x02,0x7e,0x02,0x7f,0x03,0x00,0x02,0x01,
			0x02,0x02,0x03,0xfd,0x0f,0x95,0x00,0x6b,0x00,0x8b,
			0xfe,0x02,0xfe,0x01,0xfd,0x00,0xfe,0x7f,0xfe,0x7e,
			0xff,0x7d,0x00,0x7e,0x01,0x7d,0x02,0x7e,0x02,0x7f,
			0x03,0x00,0x02,0x01,0x02,0x02,0x04,0xfd,0x03,0x88,
			0x0c,0x00,0x00,0x02,0xff,0x02,0xff,0x01,0xfe,0x01,
			0xfd,0x00,0xfe,0x7f,0xfe,0x7e,0xff,0x7d,0x00,0x7e,
			0x01,0x7d,0x02,0x7e,0x02,0x7f,0x03,0x00,0x02,0x01,
			0x02,0x02,0x03,0xfd,0x0a,0x95,0xfe,0x00,0xfe,0x7f,
			0xff,0x7d,0x00,0x6f,0xfd,0x8e,0x07,0x00,0x03,0xf2,
			0x0f,0x8e,0x00,0x70,0xff,0x7d,0xff,0x7f,0xfe,0x7f,
			0xfd,0x00,0xfe,0x01,0x09,0x91,0xfe,0x02,0xfe,0x01,
			0xfd,0x00,0xfe,0x7f,0xfe,0x7e,0xff,0x7d,0x00,0x7e,
			0x01,0x7d,0x02,0x7e,0x02,0x7f,0x03,0x00,0x02,0x01,
			0x02,0x02,0x04,0xfd,0x04,0x95,0x00,0x6b,0x00,0x8a,
			0x03,0x03,0x02,0x01,0x03,0x00,0x02,0x7f,0x01,0x7d,
			0x00,0x76,0x04,0x80,0x03,0x95,0x01,0x7f,0x01,0x01,
			0xff,0x01,0xff,0x7f,0x01,0xf9,0x00,0x72,0x04,0x80,
			0x05,0x95,0x01,0x7f,0x01,0x01,0xff,0x01,0xff,0x7f,
			0x01,0xf9,0x00,0x6f,0xff,0x7d,0xfe,0x7f,0xfe,0x00,
			0x09,0x87,0x04,0x95,0x00,0x6b,0x0a,0x8e,0xf6,0x76,
			0x04,0x84,0x07,0x78,0x02,0x80,0x04,0x95,0x00,0x6b,
			0x04,0x80,0x04,0x8e,0x00,0x72,0x00,0x8a,0x03,0x03,
			0x02,0x01,0x03,0x00,0x02,0x7f,0x01,0x7d,0x00,0x76,
			0x00,0x8a,0x03,0x03,0x02,0x01,0x03,0x00,0x02,0x7f,
			0x01,0x7d,0x00,0x76,0x04,0x80,0x04,0x8e,0x00,0x72,
			0x00,0x8a,0x03,0x03,0x02,0x01,0x03,0x00,0x02,0x7f,
			0x01,0x7d,0x00,0x76,0x04,0x80,0x08,0x8e,0xfe,0x7f,
			0xfe,0x7e,0xff,0x7d,0x00,0x7e,0x01,0x7d,0x02,0x7e,
			0x02,0x7f,0x03,0x00,0x02,0x01,0x02,0x02,0x01,0x03,
			0x00,0x02,0xff,0x03,0xfe,0x02,0xfe,0x01,0xfd,0x00,
			0x0b,0xf2,0x04,0x8e,0x00,0x6b,0x00,0x92,0x02,0x02,
			0x02,0x01,0x03,0x00,0x02,0x7f,0x02,0x7e,0x01,0x7d,
			0x00,0x7e,0xff,0x7d,0xfe,0x7e,0xfe,0x7f,0xfd,0x00,
			0xfe,0x01,0xfe,0x02,0x0f,0xfd,0x0f,0x8e,0x00,0x6b,
			0x00,0x92,0xfe,0x02,0xfe,0x01,0xfd,0x00,0xfe,0x7f,
			0xfe,0x7e,0xff,0x7d,0x00,0x7e,0x01,0x7d,0x02,0x7e,
			0x02,0x7f,0x03,0x00,0x02,0x01,0x02,0x02,0x04,0xfd,
			0x04,0x8e,0x00,0x72,0x00,0x88,0x01,0x03,0x02,0x02,
			0x02,0x01,0x03,0x00,0x01,0xf2,0x0e,0x8b,0xff,0x02,
			0xfd,0x01,0xfd,0x00,0xfd,0x7f,0xff,0x7e,0x01,0x7e,
			0x02,0x7f,0x05,0x7f,0x02,0x7f,0x01,0x7e,0x00,0x7f,
			0xff,0x7e,0xfd,0x7f,0xfd,0x00,0xfd,0x01,0xff,0x02,
			0x0e,0xfd,0x05,0x95,0x00,0x6f,0x01,0x7d,0x02,0x7f,
			0x02,0x00,0xf8,0x8e,0x07,0x00,0x03,0xf2,0x04,0x8e,
			0x00,0x76,0x01,0x7d,0x02,0x7f,0x03,0x00,0x02,0x01,
			0x03,0x03,0x00,0x8a,0x00,0x72,0x04,0x80,0x02,0x8e,
			0x06,0x72,0x06,0x8e,0xfa,0x72,0x08,0x80,0x03,0x8e,
			0x04,0x72,0x04,0x8e,0xfc,0x72,0x04,0x8e,0x04,0x72,
			0x04,0x8e,0xfc,0x72,0x07,0x80,0x03,0x8e,0x0b,0x72,
			0x00,0x8e,0xf5,0x72,0x0e,0x80,0x02,0x8e,0x06,0x72,
			0x06,0x8e,0xfa,0x72,0xfe,0x7c,0xfe,0x7e,0xfe,0x7f,
			0xff,0x00,0x0f,0x87,0x0e,0x8e,0xf5,0x72,0x00,0x8e,
			0x0b,0x00,0xf5,0xf2,0x0b,0x00,0x03,0x80,0x09,0x99,
			0xfe,0x7f,0xff,0x7f,0xff,0x7e,0x00,0x7e,0x01,0x7e,
			0x01,0x7f,0x01,0x7e,0x00,0x7e,0xfe,0x7e,0x01,0x8e,
			0xff,0x7e,0x00,0x7e,0x01,0x7e,0x01,0x7f,0x01,0x7e,
			0x00,0x7e,0xff,0x7e,0xfc,0x7e,0x04,0x7e,0x01,0x7e,
			0x00,0x7e,0xff,0x7e,0xff,0x7f,0xff,0x7e,0x00,0x7e,
			0x01,0x7e,0xff,0x8e,0x02,0x7e,0x00,0x7e,0xff,0x7e,
			0xff,0x7f,0xff,0x7e,0x00,0x7e,0x01,0x7e,0x01,0x7f,
			0x02,0x7f,0x05,0x87,0x04,0x95,0x00,0x77,0x00,0xfd,
			0x00,0x77,0x04,0x80,0x05,0x99,0x02,0x7f,0x01,0x7f,
			0x01,0x7e,0x00,0x7e,0xff,0x7e,0xff,0x7f,0xff,0x7e,
			0x00,0x7e,0x02,0x7e,0xff,0x8e,0x01,0x7e,0x00,0x7e,
			0xff,0x7e,0xff,0x7f,0xff,0x7e,0x00,0x7e,0x01,0x7e,
			0x04,0x7e,0xfc,0x7e,0xff,0x7e,0x00,0x7e,0x01,0x7e,
			0x01,0x7f,0x01,0x7e,0x00,0x7e,0xff,0x7e,0x01,0x8e,
			0xfe,0x7e,0x00,0x7e,0x01,0x7e,0x01,0x7f,0x01,0x7e,
			0x00,0x7e,0xff,0x7e,0xff,0x7f,0xfe,0x7f,0x09,0x87,
			0x03,0x86,0x00,0x02,0x01,0x03,0x02,0x01,0x02,0x00,
			0x02,0x7f,0x04,0x7d,0x02,0x7f,0x02,0x00,0x02,0x01,
			0x01,0x02,0xee,0xfe,0x01,0x02,0x02,0x01,0x02,0x00,
			0x02,0x7f,0x04,0x7d,0x02,0x7f,0x02,0x00,0x02,0x01,
			0x01,0x03,0x00,0x02,0x03,0xf4,0x10,0x80,0x03,0x80,
			0x07,0x15,0x08,0x6b,0xfe,0x85,0xf5,0x00,0x10,0xfb,
			0x0d,0x95,0xf6,0x00,0x00,0x6b,0x0a,0x00,0x02,0x02,
			0x00,0x08,0xfe,0x02,0xf6,0x00,0x0e,0xf4,0x03,0x80,
			0x00,0x15,0x0a,0x00,0x02,0x7e,0x00,0x7e,0x00,0x7d,
			0x00,0x7e,0xfe,0x7f,0xf6,0x00,0x0a,0x80,0x02,0x7e,
			0x01,0x7e,0x00,0x7d,0xff,0x7d,0xfe,0x7f,0xf6,0x00,
			0x10,0x80,0x03,0x80,0x00,0x15,0x0c,0x00,0xff,0x7e,
			0x03,0xed,0x03,0xfd,0x00,0x03,0x02,0x00,0x00,0x12,
			0x02,0x03,0x0a,0x00,0x00,0x6b,0x02,0x00,0x00,0x7d,
			0xfe,0x83,0xf4,0x00,0x11,0x80,0x0f,0x80,0xf4,0x00,
			0x00,0x15,0x0c,0x00,0xff,0xf6,0xf5,0x00,0x0f,0xf5,
			0x04,0x95,0x07,0x76,0x00,0x0a,0x07,0x80,0xf9,0x76,
			0x00,0x75,0xf8,0x80,0x07,0x0c,0x09,0xf4,0xf9,0x0c,
			0x09,0xf4,0x03,0x92,0x02,0x03,0x07,0x00,0x03,0x7d,
			0x00,0x7b,0xfc,0x7e,0x04,0x7d,0x00,0x7a,0xfd,0x7e,
			0xf9,0x00,0xfe,0x02,0x06,0x89,0x02,0x00,0x06,0xf5,
			0x03,0x95,0x00,0x6b,0x0c,0x15,0x00,0x6b,0x02,0x80,
			0x03,0x95,0x00,0x6b,0x0c,0x15,0x00,0x6b,0xf8,0x96,
			0x03,0x00,0x07,0xea,0x03,0x80,0x00,0x15,0x0c,0x80,
			0xf7,0x76,0xfd,0x00,0x03,0x80,0x0a,0x75,0x03,0x80,
			0x03,0x80,0x07,0x13,0x02,0x02,0x03,0x00,0x00,0x6b,
			0x02,0x80,0x03,0x80,0x00,0x15,0x09,0x6b,0x09,0x15,
			0x00,0x6b,0x03,0x80,0x03,0x80,0x00,0x15,0x00,0xf6,
			0x0d,0x00,0x00,0x8a,0x00,0x6b,0x03,0x80,0x07,0x80,
			0xfd,0x00,0xff,0x03,0x00,0x04,0x00,0x07,0x00,0x04,
			0x01,0x02,0x03,0x01,0x06,0x00,0x03,0x7f,0x01,0x7e,
			0x01,0x7c,0x00,0x79,0xff,0x7c,0xff,0x7d,0xfd,0x00,
			0xfa,0x00,0x0e,0x80,0x03,0x80,0x00,0x15,0x0c,0x00,
			0x00,0x6b,0x02,0x80,0x03,0x80,0x00,0x15,0x0a,0x00,
			0x02,0x7f,0x01,0x7d,0x00,0x7b,0xff,0x7e,0xfe,0x7f,
			0xf6,0x00,0x10,0xf7,0x11,0x8f,0xff,0x03,0xff,0x02,
			0xfe,0x01,0xfa,0x00,0xfd,0x7f,0xff,0x7e,0x00,0x7c,
			0x00,0x79,0x00,0x7b,0x01,0x7e,0x03,0x00,0x06,0x00,
			0x02,0x00,0x01,0x03,0x01,0x02,0x03,0xfb,0x03,0x95,
			0x0c,0x00,0xfa,0x80,0x00,0x6b,0x09,0x80,0x03,0x95,
			0x00,0x77,0x06,0x7a,0x06,0x06,0x00,0x09,0xfa,0xf1,
			0xfa,0x7a,0x0e,0x80,0x03,0x87,0x00,0x0b,0x02,0x02,
			0x03,0x00,0x02,0x7e,0x01,0x02,0x04,0x00,0x02,0x7e,
			0x00,0x75,0xfe,0x7e,0xfc,0x00,0xff,0x01,0xfe,0x7f,
			0xfd,0x00,0xfe,0x02,0x07,0x8e,0x00,0x6b,0x09,0x80,
			0x03,0x80,0x0e,0x15,0xf2,0x80,0x0e,0x6b,0x03,0x80,
			0x03,0x95,0x00,0x6b,0x0e,0x00,0x00,0x7d,0xfe,0x98,
			0x00,0x6b,0x05,0x80,0x03,0x95,0x00,0x75,0x02,0x7d,
			0x0a,0x00,0x00,0x8e,0x00,0x6b,0x02,0x80,0x03,0x95,
			0x00,0x6b,0x10,0x00,0x00,0x15,0xf8,0x80,0x00,0x6b,
			0x0a,0x80,0x03,0x95,0x00,0x6b,0x10,0x00,0x00,0x15,
			0xf8,0x80,0x00,0x6b,0x0a,0x00,0x00,0x7d,0x02,0x83,
			0x10,0x80,0x03,0x95,0x00,0x6b,0x09,0x00,0x03,0x02,
			0x00,0x08,0xfd,0x02,0xf7,0x00,0x0e,0x89,0x00,0x6b,
			0x03,0x80,0x03,0x95,0x00,0x6b,0x09,0x00,0x03,0x02,
			0x00,0x08,0xfd,0x02,0xf7,0x00,0x0e,0xf4,0x03,0x92,
			0x02,0x03,0x07,0x00,0x03,0x7d,0x00,0x70,0xfd,0x7e,
			0xf9,0x00,0xfe,0x02,0x03,0x89,0x09,0x00,0x02,0xf5,
			0x03,0x80,0x00,0x15,0x00,0xf5,0x07,0x00,0x00,0x08,
			0x02,0x03,0x06,0x00,0x02,0x7d,0x00,0x70,0xfe,0x7e,
			0xfa,0x00,0xfe,0x02,0x00,0x08,0x0c,0xf6,0x0f,0x80,
			0x00,0x15,0xf6,0x00,0xfe,0x7d,0x00,0x79,0x02,0x7e,
			0x0a,0x00,0xf4,0xf7,0x07,0x09,0x07,0xf7,0x03,0x8c,
			0x01,0x02,0x01,0x01,0x05,0x00,0x02,0x7f,0x01,0x7e,
			0x00,0x74,0x00,0x86,0xff,0x01,0xfe,0x01,0xfb,0x00,
			0xff,0x7f,0xff,0x7f,0x00,0x7c,0x01,0x7e,0x01,0x00,
			0x05,0x00,0x02,0x00,0x01,0x02,0x03,0xfe,0x04,0x8e,
			0x02,0x01,0x04,0x00,0x02,0x7f,0x01,0x7e,0x00,0x77,
			0xff,0x7e,0xfe,0x7f,0xfc,0x00,0xfe,0x01,0xff,0x02,
			0x00,0x09,0x01,0x02,0x02,0x02,0x03,0x01,0x02,0x01,
			0x01,0x01,0x01,0x02,0x02,0xeb,0x03,0x80,0x00,0x15,
			0x03,0x00,0x02,0x7e,0x00,0x7b,0xfe,0x7e,0xfd,0x00,
			0x03,0x80,0x04,0x00,0x03,0x7e,0x00,0x78,0xfd,0x7e,
			0xf9,0x00,0x0c,0x80,0x03,0x8c,0x02,0x02,0x02,0x01,
			0x03,0x00,0x02,0x7f,0x01,0x7d,0xfe,0x7e,0xf9,0x7d,
			0xff,0x7e,0x00,0x7d,0x03,0x7f,0x02,0x00,0x03,0x01,
			0x02,0x01,0x02,0xfe,0x0d,0x8c,0xff,0x02,0xfe,0x01,
			0xfc,0x00,0xfe,0x7f,0xff,0x7e,0x00,0x77,0x01,0x7e,
			0x02,0x7f,0x04,0x00,0x02,0x01,0x01,0x02,0x00,0x0f,
			0xff,0x02,0xfe,0x01,0xf9,0x00,0x0c,0xeb,0x03,0x88,
			0x0a,0x00,0x00,0x02,0x00,0x03,0xfe,0x02,0xfa,0x00,
			0xff,0x7e,0xff,0x7d,0x00,0x7b,0x01,0x7c,0x01,0x7f,
			0x06,0x00,0x02,0x02,0x03,0xfe,0x03,0x8f,0x06,0x77,
			0x06,0x09,0xfa,0x80,0x00,0x71,0xff,0x87,0xfb,0x79,
			0x07,0x87,0x05,0x79,0x02,0x80,0x03,0x8d,0x02,0x02,
			0x06,0x00,0x02,0x7e,0x00,0x7d,0xfc,0x7d,0x04,0x7e,
			0x00,0x7d,0xfe,0x7e,0xfa,0x00,0xfe,0x02,0x04,0x85,
			0x02,0x00,0x06,0xf9,0x03,0x8f,0x00,0x73,0x01,0x7e,
			0x07,0x00,0x02,0x02,0x00,0x0d,0x00,0xf3,0x01,0x7e,
			0x03,0x80,0x03,0x8f,0x00,0x73,0x01,0x7e,0x07,0x00,
			0x02,0x02,0x00,0x0d,0x00,0xf3,0x01,0x7e,0xf8,0x90,
			0x03,0x00,0x08,0xf0,0x03,0x80,0x00,0x15,0x00,0xf3,
			0x02,0x00,0x06,0x07,0xfa,0xf9,0x07,0x78,0x03,0x80,
			0x03,0x80,0x04,0x0c,0x02,0x03,0x04,0x00,0x00,0x71,
			0x02,0x80,0x03,0x80,0x00,0x0f,0x06,0x77,0x06,0x09,
			0x00,0x71,0x02,0x80,0x03,0x80,0x00,0x0f,0x0a,0xf1,
			0x00,0x0f,0xf6,0xf8,0x0a,0x00,0x02,0xf9,0x05,0x80,
			0xff,0x01,0xff,0x04,0x00,0x05,0x01,0x03,0x01,0x02,
			0x06,0x00,0x02,0x7e,0x00,0x7d,0x00,0x7b,0x00,0x7c,
			0xfe,0x7f,0xfa,0x00,0x0b,0x80,0x03,0x80,0x00,0x0f,
			0x00,0xfb,0x01,0x03,0x01,0x02,0x05,0x00,0x02,0x7e,
			0x01,0x7d,0x00,0x76,0x03,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,0x10,0x80,
			0x10,0x80,0x0a,0x8f,0x02,0x7f,0x01,0x7e,0x00,0x76,
			0xff,0x7f,0xfe,0x7f,0xfb,0x00,0xff,0x01,0xff,0x01,
			0x00,0x0a,0x01,0x02,0x01,0x01,0x05,0x00,0xf9,0x80,
			0x00,0x6b,0x0c,0x86,0x0d,0x8a,0xff,0x03,0xfe,0x02,
			0xfb,0x00,0xff,0x7e,0xff,0x7d,0x00,0x7b,0x01,0x7c,
			0x01,0x7f,0x05,0x00,0x02,0x01,0x01,0x03,0x03,0xfc,
			0x03,0x80,0x00,0x0f,0x00,0xfb,0x01,0x03,0x01,0x02,
			0x04,0x00,0x01,0x7e,0x01,0x7d,0x00,0x76,0x00,0x8a,
			0x01,0x03,0x02,0x02,0x03,0x00,0x02,0x7e,0x01,0x7d,
			0x00,0x76,0x03,0x80,0x03,0x8f,0x00,0x74,0x01,0x7e,
			0x02,0x7f,0x04,0x00,0x02,0x01,0x01,0x01,0x00,0x8d,
			0x00,0x6e,0xff,0x7e,0xfe,0x7f,0xfb,0x00,0xfe,0x01,
			0x0c,0x85,0x03,0x8d,0x01,0x02,0x03,0x00,0x02,0x7e,
			0x01,0x02,0x03,0x00,0x02,0x7e,0x00,0x74,0xfe,0x7f,
			0xfd,0x00,0xff,0x01,0xfe,0x7f,0xfd,0x00,0xff,0x01,
			0x00,0x0c,0x06,0x82,0x00,0x6b,0x08,0x86,0x03,0x80,
			0x0a,0x0f,0xf6,0x80,0x0a,0x71,0x03,0x80,0x03,0x8f,
			0x00,0x73,0x01,0x7e,0x07,0x00,0x02,0x02,0x00,0x0d,
			0x00,0xf3,0x01,0x7e,0x00,0x7e,0x03,0x82,0x03,0x8f,
			0x00,0x79,0x02,0x7e,0x08,0x00,0x00,0x89,0x00,0x71,
			0x02,0x80,0x03,0x8f,0x00,0x73,0x01,0x7e,0x03,0x00,
			0x02,0x02,0x00,0x0d,0x00,0xf3,0x01,0x7e,0x03,0x00,
			0x02,0x02,0x00,0x0d,0x00,0xf3,0x01,0x7e,0x03,0x80,
			0x03,0x8f,0x00,0x73,0x01,0x7e,0x03,0x00,0x02,0x02,
			0x00,0x0d,0x00,0xf3,0x01,0x7e,0x03,0x00,0x02,0x02,
			0x00,0x0d,0x00,0xf3,0x01,0x7e,0x00,0x7e,0x03,0x82,
			0x03,0x8d,0x00,0x02,0x02,0x00,0x00,0x71,0x08,0x00,
			0x02,0x02,0x00,0x06,0xfe,0x02,0xf8,0x00,0x0c,0xf6,
			0x03,0x8f,0x00,0x71,0x07,0x00,0x02,0x02,0x00,0x06,
			0xfe,0x02,0xf9,0x00,0x0c,0x85,0x00,0x71,0x02,0x80,
			0x03,0x8f,0x00,0x71,0x07,0x00,0x03,0x02,0x00,0x06,
			0xfd,0x02,0xf9,0x00,0x0c,0xf6,0x03,0x8d,0x02,0x02,
			0x06,0x00,0x02,0x7e,0x00,0x75,0xfe,0x7e,0xfa,0x00,
			0xfe,0x02,0x04,0x85,0x06,0x00,0x02,0xf9,0x03,0x80,
			0x00,0x0f,0x00,0xf8,0x04,0x00,0x00,0x06,0x02,0x02,
			0x04,0x00,0x02,0x7e,0x00,0x75,0xfe,0x7e,0xfc,0x00,
			0xfe,0x02,0x00,0x05,0x0a,0xf9,0x0d,0x80,0x00,0x0f,
			0xf7,0x00,0xff,0x7e,0x00,0x7b,0x01,0x7e,0x09,0x00,
			0xf6,0xfa,0x04,0x06,0x08,0xfa
		};
    };
}