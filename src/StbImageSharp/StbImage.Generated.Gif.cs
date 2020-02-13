// Generated by Sichem at 14.02.2020 1:27:28

using System;
using System.Runtime.InteropServices;

namespace StbImageSharp
{
	unsafe partial class StbImage
	{
		public static int stbi__gif_test_raw(stbi__context s)
		{
			int sz = 0;
			if ((((stbi__get8(s) != 'G') || (stbi__get8(s) != 'I')) || (stbi__get8(s) != 'F')) || (stbi__get8(s) != '8'))
				return (int)(0);
			sz = (int)(stbi__get8(s));
			if ((sz != '9') && (sz != '7'))
				return (int)(0);
			if (stbi__get8(s) != 'a')
				return (int)(0);
			return (int)(1);
		}

		public static int stbi__gif_test(stbi__context s)
		{
			int r = (int)(stbi__gif_test_raw(s));
			stbi__rewind(s);
			return (int)(r);
		}

		public static int stbi__gif_header(stbi__context s, stbi__gif g, int* comp, int is_info)
		{
			byte version = 0;
			if ((((stbi__get8(s) != 'G') || (stbi__get8(s) != 'I')) || (stbi__get8(s) != 'F')) || (stbi__get8(s) != '8'))
				return (int)(stbi__err("not GIF"));
			version = (byte)(stbi__get8(s));
			if ((version != '7') && (version != '9'))
				return (int)(stbi__err("not GIF"));
			if (stbi__get8(s) != 'a')
				return (int)(stbi__err("not GIF"));
			stbi__g_failure_reason = "";
			g.w = (int)(stbi__get16le(s));
			g.h = (int)(stbi__get16le(s));
			g.flags = (int)(stbi__get8(s));
			g.bgindex = (int)(stbi__get8(s));
			g.ratio = (int)(stbi__get8(s));
			g.transparent = (int)(-1);
			if (comp != null)
				*comp = (int)(4);
			if ((is_info) != 0)
				return (int)(1);
			if ((g.flags & 0x80) != 0)
				stbi__gif_parse_colortable(s, g.pal, (int)(2 << (g.flags & 7)), (int)(-1));
			return (int)(1);
		}

		public static int stbi__gif_info_raw(stbi__context s, int* x, int* y, int* comp)
		{
			stbi__gif g = new stbi__gif();
			if (stbi__gif_header(s, g, comp, (int)(1)) == 0)
			{
				stbi__rewind(s);
				return (int)(0);
			}

			if ((x) != null)
				*x = (int)(g.w);
			if ((y) != null)
				*y = (int)(g.h);

			return (int)(1);
		}

		public static void stbi__out_gif_code(stbi__gif g, ushort code)
		{
			byte* p;
			byte* c;
			int idx = 0;
			if ((g.codes[code].prefix) >= (0))
				stbi__out_gif_code(g, (ushort)(g.codes[code].prefix));
			if ((g.cur_y) >= (g.max_y))
				return;
			idx = (int)(g.cur_x + g.cur_y);
			p = &g._out_[idx];
			g.history[idx / 4] = (byte)(1);
			c = &g.color_table[g.codes[code].suffix * 4];
			if ((c[3]) > (128))
			{
				p[0] = (byte)(c[2]);
				p[1] = (byte)(c[1]);
				p[2] = (byte)(c[0]);
				p[3] = (byte)(c[3]);
			}

			g.cur_x += (int)(4);
			if ((g.cur_x) >= (g.max_x))
			{
				g.cur_x = (int)(g.start_x);
				g.cur_y += (int)(g.step);
				while (((g.cur_y) >= (g.max_y)) && ((g.parse) > (0)))
				{
					g.step = (int)((1 << g.parse) * g.line_size);
					g.cur_y = (int)(g.start_y + (g.step >> 1));
					--g.parse;
				}
			}

		}

		public static byte* stbi__process_gif_raster(stbi__context s, stbi__gif g)
		{
			byte lzw_cs = 0;
			int len = 0;
			int init_code = 0;
			uint first = 0;
			int codesize = 0;
			int codemask = 0;
			int avail = 0;
			int oldcode = 0;
			int bits = 0;
			int valid_bits = 0;
			int clear = 0;
			stbi__gif_lzw* p;
			lzw_cs = (byte)(stbi__get8(s));
			if ((lzw_cs) > (12))
				return (null);
			clear = (int)(1 << lzw_cs);
			first = (uint)(1);
			codesize = (int)(lzw_cs + 1);
			codemask = (int)((1 << codesize) - 1);
			bits = (int)(0);
			valid_bits = (int)(0);
			for (init_code = (int)(0); (init_code) < (clear); init_code++)
			{
				((stbi__gif_lzw*)(g.codes))[init_code].prefix = (short)(-1);
				((stbi__gif_lzw*)(g.codes))[init_code].first = ((byte)(init_code));
				((stbi__gif_lzw*)(g.codes))[init_code].suffix = ((byte)(init_code));
			}
			avail = (int)(clear + 2);
			oldcode = (int)(-1);
			len = (int)(0);
			for (; ; )
			{
				if ((valid_bits) < (codesize))
				{
					if ((len) == (0))
					{
						len = (int)(stbi__get8(s));
						if ((len) == (0))
							return g._out_;
					}
					--len;
					bits |= (int)((int)(stbi__get8(s)) << valid_bits);
					valid_bits += (int)(8);
				}
				else
				{
					int code = (int)(bits & codemask);
					bits >>= codesize;
					valid_bits -= (int)(codesize);
					if ((code) == (clear))
					{
						codesize = (int)(lzw_cs + 1);
						codemask = (int)((1 << codesize) - 1);
						avail = (int)(clear + 2);
						oldcode = (int)(-1);
						first = (uint)(0);
					}
					else if ((code) == (clear + 1))
					{
						stbi__skip(s, (int)(len));
						while ((len = (int)(stbi__get8(s))) > (0))
						{
							stbi__skip(s, (int)(len));
						}
						return g._out_;
					}
					else if (code <= avail)
					{
						if ((first) != 0)
						{
							return ((byte*)((ulong)((stbi__err("no clear code")) != 0 ? ((byte*)null) : (null))));
						}
						if ((oldcode) >= (0))
						{
							p = (stbi__gif_lzw*)g.codes + avail++;
							if ((avail) > (8192))
							{
								return ((byte*)((ulong)((stbi__err("too many codes")) != 0 ? ((byte*)null) : (null))));
							}
							p->prefix = ((short)(oldcode));
							p->first = (byte)(g.codes[oldcode].first);
							p->suffix = (byte)(((code) == (avail)) ? p->first : g.codes[code].first);
						}
						else if ((code) == (avail))
							return ((byte*)((ulong)((stbi__err("illegal code in raster")) != 0 ? ((byte*)null) : (null))));
						stbi__out_gif_code(g, (ushort)(code));
						if (((avail & codemask) == (0)) && (avail <= 0x0FFF))
						{
							codesize++;
							codemask = (int)((1 << codesize) - 1);
						}
						oldcode = (int)(code);
					}
					else
					{
						return ((byte*)((ulong)((stbi__err("illegal code in raster")) != 0 ? ((byte*)null) : (null))));
					}
				}
			}
		}

		public static byte* stbi__gif_load_next(stbi__context s, stbi__gif g, int* comp, int req_comp, byte* two_back)
		{
			int dispose = 0;
			int first_frame = 0;
			int pi = 0;
			int pcount = 0;
			first_frame = (int)(0);
			if ((g._out_) == (null))
			{
				if (stbi__gif_header(s, g, comp, (int)(0)) == 0)
					return null;
				if (stbi__mad3sizes_valid((int)(4), (int)(g.w), (int)(g.h), (int)(0)) == 0)
					return ((byte*)((ulong)((stbi__err("too large")) != 0 ? ((byte*)null) : (null))));
				pcount = (int)(g.w * g.h);
				g._out_ = (byte*)(stbi__malloc((ulong)(4 * pcount)));
				g.background = (byte*)(stbi__malloc((ulong)(4 * pcount)));
				g.history = (byte*)(stbi__malloc((ulong)(pcount)));
				if (((g._out_ == null) || (g.background == null)) || (g.history == null))
					return ((byte*)((ulong)((stbi__err("outofmem")) != 0 ? ((byte*)null) : (null))));
				CRuntime.memset(g._out_, (int)(0x00), (ulong)(4 * pcount));
				CRuntime.memset(g.background, (int)(0x00), (ulong)(4 * pcount));
				CRuntime.memset(g.history, (int)(0x00), (ulong)(pcount));
				first_frame = (int)(1);
			}
			else
			{
				dispose = (int)((g.eflags & 0x1C) >> 2);
				pcount = (int)(g.w * g.h);
				if (((dispose) == (3)) && ((two_back) == (null)))
				{
					dispose = (int)(2);
				}
				if ((dispose) == (3))
				{
					for (pi = (int)(0); (pi) < (pcount); ++pi)
					{
						if ((g.history[pi]) != 0)
						{
							CRuntime.memcpy(&g._out_[pi * 4], &two_back[pi * 4], (ulong)(4));
						}
					}
				}
				else if ((dispose) == (2))
				{
					for (pi = (int)(0); (pi) < (pcount); ++pi)
					{
						if ((g.history[pi]) != 0)
						{
							CRuntime.memcpy(&g._out_[pi * 4], &g.background[pi * 4], (ulong)(4));
						}
					}
				}
				else
				{
				}
				CRuntime.memcpy(g.background, g._out_, (ulong)(4 * g.w * g.h));
			}

			CRuntime.memset(g.history, (int)(0x00), (ulong)(g.w * g.h));
			for (; ; )
			{
				int tag = (int)(stbi__get8(s));
				switch (tag)
				{
					case 0x2C:
					{
						int x = 0;
						int y = 0;
						int w = 0;
						int h = 0;
						byte* o;
						x = (int)(stbi__get16le(s));
						y = (int)(stbi__get16le(s));
						w = (int)(stbi__get16le(s));
						h = (int)(stbi__get16le(s));
						if (((x + w) > (g.w)) || ((y + h) > (g.h)))
							return ((byte*)((ulong)((stbi__err("bad Image Descriptor")) != 0 ? ((byte*)null) : (null))));
						g.line_size = (int)(g.w * 4);
						g.start_x = (int)(x * 4);
						g.start_y = (int)(y * g.line_size);
						g.max_x = (int)(g.start_x + w * 4);
						g.max_y = (int)(g.start_y + h * g.line_size);
						g.cur_x = (int)(g.start_x);
						g.cur_y = (int)(g.start_y);
						if ((w) == (0))
							g.cur_y = (int)(g.max_y);
						g.lflags = (int)(stbi__get8(s));
						if ((g.lflags & 0x40) != 0)
						{
							g.step = (int)(8 * g.line_size);
							g.parse = (int)(3);
						}
						else
						{
							g.step = (int)(g.line_size);
							g.parse = (int)(0);
						}
						if ((g.lflags & 0x80) != 0)
						{
							stbi__gif_parse_colortable(s, g.lpal, (int)(2 << (g.lflags & 7)), (int)((g.eflags & 0x01) != 0 ? g.transparent : -1));
							g.color_table = (byte*)(g.lpal);
						}
						else if ((g.flags & 0x80) != 0)
						{
							g.color_table = (byte*)(g.pal);
						}
						else
							return ((byte*)((ulong)((stbi__err("missing color table")) != 0 ? ((byte*)null) : (null))));
						o = stbi__process_gif_raster(s, g);
						if (o == null)
							return (null);
						pcount = (int)(g.w * g.h);
						if (((first_frame) != 0) && ((g.bgindex) > (0)))
						{
							for (pi = (int)(0); (pi) < (pcount); ++pi)
							{
								if ((g.history[pi]) == (0))
								{
									g.pal[g.bgindex * 4 + 3] = (byte)(255);
									CRuntime.memcpy(&g._out_[pi * 4], &g.pal[g.bgindex], (ulong)(4));
								}
							}
						}
						return o;
					}
					case 0x21:
					{
						int len = 0;
						int ext = (int)(stbi__get8(s));
						if ((ext) == (0xF9))
						{
							len = (int)(stbi__get8(s));
							if ((len) == (4))
							{
								g.eflags = (int)(stbi__get8(s));
								g.delay = (int)(10 * stbi__get16le(s));
								if ((g.transparent) >= (0))
								{
									g.pal[g.transparent * 4 + 3] = (byte)(255);
								}
								if ((g.eflags & 0x01) != 0)
								{
									g.transparent = (int)(stbi__get8(s));
									if ((g.transparent) >= (0))
									{
										g.pal[g.transparent * 4 + 3] = (byte)(0);
									}
								}
								else
								{
									stbi__skip(s, (int)(1));
									g.transparent = (int)(-1);
								}
							}
							else
							{
								stbi__skip(s, (int)(len));
								break;
							}
						}
						while ((len = (int)(stbi__get8(s))) != 0)
						{
							stbi__skip(s, (int)(len));
						}
						break;
					}
					case 0x3B:
						return null;
					default:
						return ((byte*)((ulong)((stbi__err("unknown code")) != 0 ? ((byte*)null) : (null))));
				}
			}
		}

		public static void* stbi__load_gif_main(stbi__context s, int** delays, int* x, int* y, int* z, int* comp, int req_comp)
		{
			if ((stbi__gif_test(s)) != 0)
			{
				int layers = (int)(0);
				byte* u = null;
				byte* _out_ = null;
				byte* two_back = null;
				stbi__gif g = new stbi__gif();
				int stride = 0;
				if ((delays) != null)
				{
					*delays = null;
				}
				do
				{
					u = stbi__gif_load_next(s, g, comp, (int)(req_comp), two_back);
					if ((u) != null)
					{
						*x = (int)(g.w);
						*y = (int)(g.h);
						++layers;
						stride = (int)(g.w * g.h * 4);
						if ((_out_) != null)
						{
							_out_ = (byte*)(CRuntime.realloc(_out_, (ulong)(layers * stride)));
							if ((delays) != null)
							{
								*delays = (int*)(CRuntime.realloc(*delays, (ulong)(sizeof(int) * layers)));
							}
						}
						else
						{
							_out_ = (byte*)(stbi__malloc((ulong)(layers * stride)));
							if ((delays) != null)
							{
								*delays = (int*)(stbi__malloc((ulong)(layers * sizeof(int))));
							}
						}
						CRuntime.memcpy(_out_ + ((layers - 1) * stride), u, (ulong)(stride));
						if ((layers) >= (2))
						{
							two_back = _out_ - 2 * stride;
						}
						if ((delays) != null)
						{
							(*delays)[layers - 1U] = (int)(g.delay);
						}
					}
				}
				while (u != null);
				CRuntime.free(g._out_);
				CRuntime.free(g.history);
				CRuntime.free(g.background);
				if (((req_comp) != 0) && (req_comp != 4))
					_out_ = stbi__convert_format(_out_, (int)(4), (int)(req_comp), (uint)(layers * g.w), (uint)(g.h));
				*z = (int)(layers);
				return _out_;
			}
			else
			{
				return ((byte*)((ulong)((stbi__err("not GIF")) != 0 ? ((byte*)null) : (null))));
			}

		}

		public static void* stbi__gif_load(stbi__context s, int* x, int* y, int* comp, int req_comp, stbi__result_info* ri)
		{
			byte* u = null;
			stbi__gif g = new stbi__gif();

			u = stbi__gif_load_next(s, g, comp, (int)(req_comp), null);
			if ((u) != null)
			{
				*x = (int)(g.w);
				*y = (int)(g.h);
				if (((req_comp) != 0) && (req_comp != 4))
					u = stbi__convert_format(u, (int)(4), (int)(req_comp), (uint)(g.w), (uint)(g.h));
			}
			else if ((g._out_) != null)
			{
				CRuntime.free(g._out_);
			}

			CRuntime.free(g.history);
			CRuntime.free(g.background);
			return u;
		}

		public static int stbi__gif_info(stbi__context s, int* x, int* y, int* comp)
		{
			return (int)(stbi__gif_info_raw(s, x, y, comp));
		}
	}
}