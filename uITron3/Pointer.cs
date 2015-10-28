using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public class pointer
	{
		public const int length = 4;

		private byte[] m_Data;
		private int m_Offset;

		public pointer(pointer src)
		{
			m_Data = src.m_Data;
			m_Offset = src.m_Offset;
		}

		public pointer(pointer src, int offset)
		{
			m_Data = src.m_Data;
			m_Offset = src.m_Offset + offset;
		}

		public pointer(byte[] data, int offset)
		{
			m_Data = data;
			m_Offset = offset;
		}

		public byte[] data { get { return m_Data; } }

		public int offset { get { return m_Offset; } }

		public static pointer operator +(pointer a, int b)
		{
			return new pointer(a, b);
		}

		public static pointer operator -(pointer a, int b)
		{
			return new pointer(a, -b);
		}

		public static int operator -(pointer a, pointer b)
		{
			if (a.m_Data != b.m_Data)
				throw new ArgumentException();

			return a.m_Offset - b.m_Offset;
		}

		public static pointer operator ++(pointer a)
		{
			a.m_Offset++;
			return a;
		}

		public static pointer operator --(pointer a)
		{
			a.m_Offset--;
			return a;
		}

		public static bool operator ==(pointer a, pointer b)
		{
			if (((object)a == null) && ((object)b == null))
				return true;

			if ((object)a == null)
				return false;

			if ((object)b == null)
				return false;

			if (a.m_Data != b.m_Data)
				throw new ArgumentException();

			return a.m_Offset == b.m_Offset;
		}

		public static bool operator !=(pointer a, pointer b)
		{
			if (((object)a == null) && ((object)b == null))
				return false;

			if ((object)a == null)
				return true;

			if ((object)b == null)
				return true;

			if (a.m_Data != b.m_Data)
				throw new ArgumentException();

			return a.m_Offset != b.m_Offset;
		}

		public override bool Equals(object obj)
		{
			pointer a = obj as pointer;
			if (a == null)
				return base.Equals(obj);

			return m_Data == a.m_Data && m_Offset == a.m_Offset;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator >(pointer a, pointer b)
		{
			if (a.m_Data != b.m_Data)
				throw new ArgumentException();

			return a.m_Offset > b.m_Offset;
		}

		public static bool operator >=(pointer a, pointer b)
		{
			if (a.m_Data != b.m_Data)
				throw new ArgumentException();

			return a.m_Offset >= b.m_Offset;
		}

		public static bool operator <(pointer a, pointer b)
		{
			if (a.m_Data != b.m_Data)
				throw new ArgumentException();

			return a.m_Offset < b.m_Offset;
		}

		public static bool operator <=(pointer a, pointer b)
		{
			if (a.m_Data != b.m_Data)
				throw new ArgumentException();

			return a.m_Offset <= b.m_Offset;
		}

		public static explicit operator sbyte(pointer This)
		{
			return (sbyte)This.m_Data[This.m_Offset];
		}

		public static explicit operator short(pointer This)
		{
			return BitConverter.ToInt16(This.m_Data, This.m_Offset);
		}

		public static explicit operator int(pointer This)
		{
			return BitConverter.ToInt32(This.m_Data, This.m_Offset);
		}

		public static explicit operator long(pointer This)
		{
			return BitConverter.ToInt64(This.m_Data, This.m_Offset);
		}

		public static explicit operator byte(pointer This)
		{
			return This.m_Data[This.m_Offset];
		}

		public static explicit operator ushort(pointer This)
		{
			return BitConverter.ToUInt16(This.m_Data, This.m_Offset);
		}

		public static explicit operator uint(pointer This)
		{
			return BitConverter.ToUInt32(This.m_Data, This.m_Offset);
		}

		public static explicit operator ulong(pointer This)
		{
			return BitConverter.ToUInt64(This.m_Data, This.m_Offset);
		}

		public static explicit operator float(pointer This)
		{
			return BitConverter.ToSingle(This.m_Data, This.m_Offset);
		}

		public static explicit operator double(pointer This)
		{
			return BitConverter.ToDouble(This.m_Data, This.m_Offset);
		}

		public static explicit operator string(pointer This)
		{
			return This.ToString();
		}

		public override string ToString()
		{
			int len = strlen(this);
			if (len == -1)
				len = m_Data.Length - m_Offset;
			return Encoding.Default.GetString(m_Data, m_Offset, len);
		}

		public byte this[int index]
		{
			get { return m_Data[m_Offset + index]; }
			set { m_Data[m_Offset + index] = value; }
		}

		public void SetValue(short value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(short));
		}

		public void SetValue(int value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(int));
		}

		public void SetValue(long value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(long));
		}

		public void SetValue(ushort value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(ushort));
		}

		public void SetValue(uint value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(uint));
		}

		public void SetValue(ulong value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(ulong));
		}

		public void SetValue(float value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(float));
		}

		public void SetValue(double value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_Data, m_Offset, sizeof(double));
		}

		public void SetReference(pointer value)
		{
			if (m_Data != value.data)
				throw new ArgumentException();
			SetValue(value.offset);
		}

		public static void memcpy(pointer dst, pointer src, int len)
		{
			Buffer.BlockCopy(src.m_Data, src.m_Offset, dst.m_Data, dst.m_Offset, len);
		}

		public static void memset(pointer dst, byte value, int len)
		{
			int end = dst.m_Offset + len;
			for (int i = dst.m_Offset; i < end; i++) {
				dst.m_Data[i] = value;
			}
		}

		public static int memcmp(pointer p1, pointer p2, int len)
		{
			int pos1 = p1.m_Offset, pos2 = p2.m_Offset;
			int end = pos1 + len;
			for (; pos1 < end; pos1++, pos2++) {
				int diff = p2.m_Data[pos2] - p1.m_Data[pos1];
				if (diff != 0)
					return diff;
			}
			return 0;
		}

		public static int strlen(pointer This)
		{
			int result = Array.FindIndex(This.m_Data, This.m_Offset, (p) => p == 0);
			if (result == -1)
				return -1;
			return result - This.m_Offset;
		}

		public T GetFieldValue<T>(field_info_i<T> fi)
		{
			return fi.get(this);
		}

		public void SetFieldValue<T>(field_info_i<T> fi, T value)
		{
			fi.set(this, value);
		}

		public array<T> GetArrayField<T>(array_field_info<T> fi) where T : struct
		{
			return new array<T>(this, fi);
		}

		public interface field_info_i<T>
		{
			int offset { get; }
			int size { get; }
			int count { get; }
			int bit_offset { get; }
			int bit_size { get; }
			T get(pointer owner);
			void set(pointer owner, T value);
		}

		public class value_field_info<T> : field_info_i<T> where T : struct
		{
			protected delegate object getter_t(pointer owner, int index);
			protected delegate void setter_t(pointer owner, object value, int index);

			protected int _offset;
			protected int _size;
			protected getter_t _getter;
			protected setter_t _setter;
			protected bool _bigendian;

			public value_field_info(int offset, bool bigendian = false)
			{
				Type type = typeof(T);
				_offset = offset;
				_bigendian = bigendian;

				if (type == typeof(byte)) {
					_size = sizeof(byte);
					_getter = get_byte;
					_setter = set_byte;
				}
				else if (type == typeof(sbyte)) {
					_size = sizeof(sbyte);
					_getter = get_sbyte;
					_setter = set_sbyte;
				}
				else if (BitConverter.IsLittleEndian == !bigendian) {
					if (type == typeof(short)) {
						_size = sizeof(short);
						_getter = get_short;
						_setter = set_short;
					}
					else if (type == typeof(ushort)) {
						_size = sizeof(ushort);
						_getter = get_ushort;
						_setter = set_ushort;
					}
					else if (type == typeof(int)) {
						_size = sizeof(int);
						_getter = get_int;
						_setter = set_int;
					}
					else if (type == typeof(uint)) {
						_size = sizeof(uint);
						_getter = get_uint;
						_setter = set_uint;
					}
					else if (type == typeof(long)) {
						_size = sizeof(long);
						_getter = get_long;
						_setter = set_long;
					}
					else if (type == typeof(ulong)) {
						_size = sizeof(ulong);
						_getter = get_ulong;
						_setter = set_ulong;
					}
					else {
						throw new ArgumentException();
					}
				}
				else {
					if (type == typeof(short)) {
						_size = sizeof(short);
						_getter = get_short_swp;
						_setter = set_short_swp;
					}
					else if (type == typeof(ushort)) {
						_size = sizeof(ushort);
						_getter = get_ushort_swp;
						_setter = set_ushort_swp;
					}
					else if (type == typeof(int)) {
						_size = sizeof(int);
						_getter = get_int_swp;
						_setter = set_int_swp;
					}
					else if (type == typeof(uint)) {
						_size = sizeof(uint);
						_getter = get_uint_swp;
						_setter = set_uint_swp;
					}
					else if (type == typeof(long)) {
						_size = sizeof(long);
						_getter = get_long_swp;
						_setter = set_long_swp;
					}
					else if (type == typeof(ulong)) {
						_size = sizeof(ulong);
						_getter = get_ulong_swp;
						_setter = set_ulong_swp;
					}
					else {
						throw new ArgumentException();
					}
				}
			}

			public int offset { get { return _offset; } }
			public int size { get { return _size; } }
			public virtual int count { get { return 0; } }
			public int bit_offset { get { return 0; } }
			public int bit_size { get { return 8 * _size; } }
			public bool bigendian { get { return _bigendian; } }

			public T get(pointer owner)
			{
				return (T)_getter(owner, 0);
			}

			public void set(pointer owner, T value)
			{
				_setter(owner, (T)value, 0);
			}

			private object get_byte(pointer owner, int index)
			{
				return owner.data[owner.offset + _offset + (index * _size)];
			}

			private void set_byte(pointer owner, object value, int index)
			{
				owner.data[owner.offset + _offset + (index * _size)] = (byte)value;
			}

			private object get_sbyte(pointer owner, int index)
			{
				return owner.data[owner.offset + _offset + (index * _size)];
			}

			private void set_sbyte(pointer owner, object value, int index)
			{
				owner.data[owner.offset + _offset + (index * _size)] = (byte)value;
			}

			private object get_short(pointer owner, int index)
			{
				return BitConverter.ToInt16(owner.data, owner.offset + _offset + (index * _size));
			}

			private void set_short(pointer owner, object value, int index)
			{
				Buffer.BlockCopy(BitConverter.GetBytes((short)value), 0, owner.data, owner.offset + _offset + (index * _size), sizeof(short));
			}

			private object get_ushort(pointer owner, int index)
			{
				return BitConverter.ToUInt16(owner.data, owner.offset + _offset + (index * _size));
			}

			private void set_ushort(pointer owner, object value, int index)
			{
				Buffer.BlockCopy(BitConverter.GetBytes((ushort)value), 0, owner.data, owner.offset + _offset + (index * _size), sizeof(ushort));
			}

			private object get_int(pointer owner, int index)
			{
				return BitConverter.ToInt32(owner.data, owner.offset + _offset + (index * _size));
			}

			private void set_int(pointer owner, object value, int index)
			{
				Buffer.BlockCopy(BitConverter.GetBytes((int)value), 0, owner.data, owner.offset + _offset + (index * _size), sizeof(int));
			}

			private object get_uint(pointer owner, int index)
			{
				return BitConverter.ToUInt32(owner.data, owner.offset + _offset + (index * _size));
			}

			private void set_uint(pointer owner, object value, int index)
			{
				Buffer.BlockCopy(BitConverter.GetBytes((uint)value), 0, owner.data, owner.offset + _offset + (index * _size), sizeof(uint));
			}

			private object get_long(pointer owner, int index)
			{
				return BitConverter.ToInt64(owner.data, owner.offset + _offset + (index * _size));
			}

			private void set_long(pointer owner, object value, int index)
			{
				Buffer.BlockCopy(BitConverter.GetBytes((long)value), 0, owner.data, owner.offset + _offset + (index * _size), sizeof(long));
			}

			private object get_ulong(pointer owner, int index)
			{
				return BitConverter.ToUInt64(owner.data, owner.offset + _offset + (index * _size));
			}

			private void set_ulong(pointer owner, object value, int index)
			{
				Buffer.BlockCopy(BitConverter.GetBytes((ulong)value), 0, owner.data, owner.offset + _offset + (index * _size), sizeof(ulong));
			}

			private object get_short_swp(pointer owner, int index)
			{
				byte[] temp = new byte[sizeof(short)];
				Buffer.BlockCopy(owner.data, owner.offset + _offset + (index * _size), temp, 0, sizeof(short));
				Array.Reverse(temp);
				return BitConverter.ToInt16(temp, 0);
			}

			private void set_short_swp(pointer owner, object value, int index)
			{
				byte[] temp = BitConverter.GetBytes((short)value);
				Array.Reverse(temp);
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset + (index * _size), sizeof(short));
			}

			private object get_ushort_swp(pointer owner, int index)
			{
				byte[] temp = new byte[sizeof(ushort)];
				Buffer.BlockCopy(owner.data, owner.offset + _offset + (index * _size), temp, 0, sizeof(ushort));
				Array.Reverse(temp);
				return BitConverter.ToUInt16(temp, 0);
			}

			private void set_ushort_swp(pointer owner, object value, int index)
			{
				byte[] temp = BitConverter.GetBytes((ushort)value);
				Array.Reverse(temp);
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset + (index * _size), sizeof(ushort));
			}

			private object get_int_swp(pointer owner, int index)
			{
				byte[] temp = new byte[sizeof(int)];
				Buffer.BlockCopy(owner.data, owner.offset + _offset + (index * _size), temp, 0, sizeof(int));
				Array.Reverse(temp);
				return BitConverter.ToInt32(temp, 0);
			}

			private void set_int_swp(pointer owner, object value, int index)
			{
				byte[] temp = BitConverter.GetBytes((int)value);
				Array.Reverse(temp);
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset + (index * _size), sizeof(int));
			}

			private object get_uint_swp(pointer owner, int index)
			{
				byte[] temp = new byte[sizeof(uint)];
				Buffer.BlockCopy(owner.data, owner.offset + _offset + (index * _size), temp, 0, sizeof(uint));
				Array.Reverse(temp);
				return BitConverter.ToUInt32(temp, 0);
			}

			private void set_uint_swp(pointer owner, object value, int index)
			{
				byte[] temp = BitConverter.GetBytes((uint)value);
				Array.Reverse(temp);
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset + (index * _size), sizeof(uint));
			}

			private object get_long_swp(pointer owner, int index)
			{
				byte[] temp = new byte[sizeof(long)];
				Buffer.BlockCopy(owner.data, owner.offset + _offset + (index * _size), temp, 0, sizeof(long));
				Array.Reverse(temp);
				return BitConverter.ToInt64(temp, 0);
			}

			private void set_long_swp(pointer owner, object value, int index)
			{
				byte[] temp = BitConverter.GetBytes((long)value);
				Array.Reverse(temp);
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset + (index * _size), sizeof(long));
			}

			private object get_ulong_swp(pointer owner, int index)
			{
				byte[] temp = new byte[sizeof(ulong)];
				Buffer.BlockCopy(owner.data, owner.offset + _offset + (index * _size), temp, 0, sizeof(ulong));
				Array.Reverse(temp);
				return BitConverter.ToUInt64(temp, 0);
			}

			private void set_ulong_swp(pointer owner, object value, int index)
			{
				byte[] temp = BitConverter.GetBytes((ulong)value);
				Array.Reverse(temp);
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset + (index * _size), sizeof(ulong));
			}
		}

		public class array_field_info<T> : value_field_info<T> where T : struct
		{
			int _count;

			public array_field_info(int offset, int count, bool bigendian = false)
				: base(offset, bigendian)
			{
				_count = count;
			}

			public override int count { get { return _count; } }

			public T get(pointer owner, int index)
			{
				return (T)_getter(owner, index);
			}

			public void set(pointer owner, int index, T value)
			{
				_setter(owner, (T)value, index);
			}
		}

		public class bit_field_info<T> : field_info_i<T> where T : struct
		{
			delegate object getter_t(pointer owner);
			delegate void setter_t(pointer owner, object value);

			int _offset;
			int _size;
			int _bit_offset;
			int _bit_size;
			getter_t _getter;
			setter_t _setter;
			ulong _mask;

			public bit_field_info(int offset, int bit_offset, int bit_size, bool bigendian = false)
			{
				_offset = offset;
				_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
				_bit_offset = bit_offset;
				_bit_size = bit_size;

				if ((bit_offset > (8 * _size)) || (bit_size > (8 * _size)))
					throw new ArgumentException();

				if (BitConverter.IsLittleEndian == !bigendian) {
					_mask = (~0ul >> (64 - bit_size)) << bit_offset;

					if (_size <= 1) {
						_getter = get_byte;
						_setter = set_byte;
					}
					else if (_size <= 2) {
						_getter = get_ushort;
						_setter = set_ushort;
					}
					else if (_size <= 4) {
						_getter = get_uint;
						_setter = set_uint;
					}
					else if (_size <= 8) {
						_getter = get_ulong;
						_setter = set_ulong;
					}
					else {
						throw new ArgumentException();
					}
				}
				else {
					_mask = (~0ul << (64 - bit_size)) >> bit_offset;
					byte[] temp = BitConverter.GetBytes(_mask);
					Array.Reverse(temp);
					_mask = BitConverter.ToUInt64(temp, 0);

					if (_size <= 1) {
						_getter = get_byte;
						_setter = set_byte;
					}
					else if (_size <= 2) {
						_getter = get_ushort_swp;
						_setter = set_ushort_swp;
					}
					else if (_size <= 4) {
						_getter = get_uint_swp;
						_setter = set_uint_swp;
					}
					else if (_size <= 8) {
						_getter = get_ulong_swp;
						_setter = set_ulong_swp;
					}
					else {
						throw new ArgumentException();
					}
				}
			}

			public int offset { get { return _offset; } }
			public int size { get { return _size; } }
			public int count { get { return 0; } }
			public int bit_offset { get { return _bit_offset; } }
			public int bit_size { get { return _bit_size; } }

			public T get(pointer owner)
			{
				return (T)_getter(owner);
			}

			public void set(pointer owner, T value)
			{
				_setter(owner, (T)value);
			}

			private object get_byte(pointer owner)
			{
				return (byte)((owner.data[owner.offset + _offset] & _mask) >> _bit_offset);
			}

			private void set_byte(pointer owner, object value)
			{
				owner.data[owner.offset + _offset] = (byte)((((byte)value << _bit_offset) & (byte)_mask)
					| (owner.data[owner.offset + _offset] & (byte)~_mask));
			}

			private object get_ushort(pointer owner)
			{
				return (ushort)((BitConverter.ToUInt16(owner.data, owner.offset + _offset) & _mask) >> _bit_offset);
			}

			private void set_ushort(pointer owner, object value)
			{
				ushort temp = (ushort)((((ushort)value << _bit_offset) & (ushort)_mask)
					| (BitConverter.ToUInt16(owner.data, owner.offset + _offset) & (ushort)~_mask));

				Buffer.BlockCopy(BitConverter.GetBytes(temp), 0, owner.data, owner.offset + _offset, sizeof(ushort));
			}

			private object get_uint(pointer owner)
			{
				return (uint)((BitConverter.ToUInt32(owner.data, owner.offset + _offset) & _mask) >> _bit_offset);
			}

			private void set_uint(pointer owner, object value)
			{
				uint temp = (uint)((((uint)value << _bit_offset) & (uint)_mask)
					| (BitConverter.ToUInt16(owner.data, owner.offset + _offset) & (uint)~_mask));

				Buffer.BlockCopy(BitConverter.GetBytes(temp), 0, owner.data, owner.offset + _offset, sizeof(uint));
			}

			private object get_ulong(pointer owner)
			{
				return (ulong)((BitConverter.ToUInt64(owner.data, owner.offset + _offset) & _mask) >> _bit_offset);
			}

			private void set_ulong(pointer owner, object value)
			{
				ulong temp = (ulong)((((ulong)value << _bit_offset) & (ulong)_mask)
					| (BitConverter.ToUInt16(owner.data, owner.offset + _offset) & (ulong)~_mask));

				Buffer.BlockCopy(BitConverter.GetBytes(temp), 0, owner.data, owner.offset + _offset, sizeof(ulong));
			}

			private object get_ushort_swp(pointer owner)
			{
				byte[] temp = BitConverter.GetBytes(
					BitConverter.ToUInt16(owner.data, owner.offset + _offset) & _mask);
				Array.Reverse(temp);
				return (ushort)(BitConverter.ToUInt16(temp, 0) >> (16 - _bit_offset - _bit_size));
			}

			private void set_ushort_swp(pointer owner, object value)
			{
				byte[] mskval = BitConverter.GetBytes((ushort)((((ushort)value << _bit_offset) & (ushort)_mask) << (16 - _bit_offset - _bit_size)));
				Array.Reverse(mskval);
				byte[] temp = BitConverter.GetBytes((ushort)(BitConverter.ToUInt16(mskval, 0)
					| (BitConverter.ToUInt16(owner.data, owner.offset + _offset) & ~_mask)));
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset, sizeof(ushort));
			}

			private object get_uint_swp(pointer owner)
			{
				byte[] temp = BitConverter.GetBytes(
					BitConverter.ToUInt32(owner.data, owner.offset + _offset) & _mask);
				Array.Reverse(temp);
				return (uint)(BitConverter.ToUInt32(temp, 0) >> (32 - _bit_offset - _bit_size));
			}

			private void set_uint_swp(pointer owner, object value)
			{
				byte[] mskval = BitConverter.GetBytes((((uint)value << _bit_offset) & (uint)_mask) << (32 - _bit_offset - _bit_size));
				Array.Reverse(mskval);
				byte[] temp = BitConverter.GetBytes((uint)(BitConverter.ToUInt32(mskval, 0)
					| (BitConverter.ToUInt32(owner.data, owner.offset + _offset) & ~_mask)));
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset, sizeof(uint));
			}

			private object get_ulong_swp(pointer owner)
			{
				byte[] temp = BitConverter.GetBytes(
					BitConverter.ToUInt64(owner.data, owner.offset + _offset) & _mask);
				Array.Reverse(temp);
				return BitConverter.ToUInt64(temp, 0) >> (64 - _bit_offset - _bit_size);
			}

			private void set_ulong_swp(pointer owner, object value)
			{
				byte[] mskval = BitConverter.GetBytes((((ulong)value << _bit_offset) & (ulong)_mask) << (64 - _bit_offset - _bit_size));
				Array.Reverse(mskval);
				byte[] temp = BitConverter.GetBytes((ulong)(BitConverter.ToUInt64(mskval, 0)
					| (BitConverter.ToUInt64(owner.data, owner.offset + _offset) & ~_mask)));
				Buffer.BlockCopy(temp, 0, owner.data, owner.offset + _offset, sizeof(ulong));
			}
		}

		public class bool_field_info : field_info_i<bool>
		{
			int _offset;
			int _bit;

			public bool_field_info(int offset, int bit)
			{
				_offset = offset;
				_bit = bit;
			}

			public int size { get { return 1; } }

			public bool get(pointer owner)
			{
				return (owner.data[owner.offset + _offset] & (1 << _bit)) != 0;
			}

			public void set(pointer owner, bool value)
			{
				if (value)
					owner.data[owner.offset + _offset] |= (byte)(1 << _bit);
				else
					owner.data[owner.offset + _offset] &= (byte)~(1 << _bit);
			}

			public int offset { get { return _offset; } }
			public int count { get { return 0; } }
			public int bit_offset { get { return 0; } }
			public int bit_size { get { return 8; } }
		}

		public class pointer_field_info : field_info_i<pointer>
		{
			int _offset;

			public pointer_field_info(int offset)
			{
				_offset = offset;
			}

			public int offset { get { return _offset; } }
			public int size { get { return pointer.length; } }
			public int count { get { return 0; } }
			public int bit_offset { get { return 0; } }
			public int bit_size { get { return 8 * size; } }

			public pointer get(pointer owner)
			{
				int address = BitConverter.ToInt32(owner.data, owner.offset + _offset);
				if (address < 0)
					return null;

				return new pointer(owner.data, address);
			}

			public void set(pointer owner, pointer value)
			{
				if (owner.data != value.data)
					throw new ArgumentException();

				int address = -1;
				if (value != null)
					address = value.offset;
				Buffer.BlockCopy(BitConverter.GetBytes(address), 0, owner.data, owner.offset + _offset, sizeof(int));
			}
		}

		public class struct_field_info<T> : field_info_i<T> where T : pointer
		{
			int _offset;
			int _size;

			public struct_field_info(int offset)
			{
				_offset = offset;

				System.Reflection.FieldInfo fi = typeof(T).GetField("length");
				_size = (int)fi.GetValue(null);
			}

			public int offset { get { return _offset; } }
			public int size { get { return _size; } }
			public int count { get { return 0; } }
			public int bit_offset { get { return 0; } }
			public int bit_size { get { return 8 * size; } }

			public T get(pointer owner)
			{
				System.Reflection.ConstructorInfo ci;

				ci = typeof(T).GetConstructor(new Type[] { typeof(byte[]), typeof(int) });
				if (ci != null)
					return (T)ci.Invoke(new object[] { owner.m_Data, owner.offset + _offset });

				ci = typeof(T).GetConstructor(new Type[] { typeof(pointer), typeof(int) });
				return (T)ci.Invoke(new object[] { owner, owner.data, owner.offset + _offset });
			}

			public void set(pointer owner, T value)
			{
				memcpy(owner, value, _size);
			}
		}

		public class pointer_field_info<T> : field_info_i<T> where T : pointer
		{
			int _offset;

			public pointer_field_info(int offset)
			{
				_offset = offset;
			}

			public int offset { get { return _offset; } }
			public int size { get { return pointer.length; } }
			public int count { get { return 0; } }
			public int bit_offset { get { return 0; } }
			public int bit_size { get { return 8 * size; } }

			public T get(pointer owner)
			{
				System.Reflection.ConstructorInfo ci;

				ci = typeof(T).GetConstructor(new Type[] { typeof(byte[]), typeof(int) });
				if (ci != null)
					return (T)ci.Invoke(new object[] { owner.m_Data, BitConverter.ToInt32(owner.data, owner.offset + _offset) });

				ci = typeof(T).GetConstructor(new Type[] { typeof(pointer), typeof(int) });
				return (T)ci.Invoke(new object[] { owner, BitConverter.ToInt32(owner.data, owner.offset + _offset) });
			}

			public void set(pointer owner, T value)
			{
				if (owner.data != value.data)
					throw new ArgumentException();

				Buffer.BlockCopy(BitConverter.GetBytes(value.m_Offset), 0, owner.data, owner.offset + _offset, sizeof(int));
			}
		}

		public static int atoi(pointer servname)
		{
			throw new NotImplementedException();
		}

		public static int strcmp(pointer pointer, pointer name)
		{
			throw new NotImplementedException();
		}

		public static int lengthof<T>(T obj) where T : pointer
		{
			System.Reflection.FieldInfo fi = typeof(T).GetField("length");
			return (int)fi.GetValue(null);
		}

		public static int lengthof(Type t)
		{
			System.Reflection.FieldInfo fi = t.GetField("length");
			return (int)fi.GetValue(null);
		}

		public string ToString(int pos, int len)
		{
			return Encoding.UTF8.GetString(m_Data, m_Offset + pos, len);
		}
	}

	public class refptr<T> : pointer where T : pointer
	{
		public refptr(byte[] src, int offset) : base(src, offset) { }
		public refptr(pointer src) : base(src) { }
		public refptr(pointer src, int offset) : base(src, offset) { }

		public new T this[int index]
		{
			get
			{
				int offset = (int)(new pointer(this, sizeof(int) * index));
				if (offset == 0)
					return null;
				System.Reflection.ConstructorInfo ci = typeof(T).GetConstructor(new Type[] { typeof(byte[]), typeof(int) });
				return (T)ci.Invoke(new object[] { data, offset });
			}
			set
			{
				if (data != value.data)
					throw new ArgumentException();

				pointer dst = new pointer(this, sizeof(int) * index);
				pointer src = new pointer(BitConverter.GetBytes(value.offset), 0);
				pointer.memcpy(dst, src, sizeof(int));
			}
		}
	}

	public class array<T> : pointer where T : struct
	{
		array_field_info<T> _info;

		public array(pointer src, array_field_info<T> info)
			: base(src, info.offset)
		{
			_info = new array_field_info<T>(0, info.count, info.bigendian);
		}

		public new T this[int index]
		{
			get { return (T)_info.get(this, index); }
			set { _info.set(this, index, value); }
		}
	}
}
