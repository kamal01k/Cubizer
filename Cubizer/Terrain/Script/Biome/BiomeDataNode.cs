﻿using System;

namespace Cubizer
{
	[Serializable]
	public class BiomeDataNode<_Tx, _Ty>
		where _Tx : struct
		where _Ty : class
	{
		public _Tx position;
		public _Ty value;

		public BiomeDataNode()
		{
			value = null;
		}

		public BiomeDataNode(_Tx x, _Ty value)
		{
			position = x;
			this.value = value;
		}

		public bool is_empty()
		{
			return value == null;
		}
	}
}