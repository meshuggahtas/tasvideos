﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data
{
	public class SystemPageOf<T> : SystemPagedModel, IEnumerable<T>
	{
		private readonly IEnumerable<T> _items;

		public SystemPageOf(IEnumerable<T> items)
		{
			_items = items;
		}

		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}

	public class SystemPagedModel : PagedModel
	{
		[Display(Name = "System")]
		public string SystemCode { get; set; }
	}

	public class PageOf<T> : PagedModel, IEnumerable<T>
	{
		private readonly IEnumerable<T> _items;

		public PageOf(IEnumerable<T> items)
		{
			_items = items;
		}

		public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}

	public class PagedModel : PagingModel
	{
		public int RowCount { get; set; }

		public int LastPage => (int)Math.Ceiling(RowCount / (double)PageSize);
		public int StartRow => ((CurrentPage - 1) * PageSize) + 1;
		public int LastRow => Math.Min(RowCount, StartRow + PageSize - 1);
	}

	/// <summary>
	/// Represents all of the data necessary to create a paged query
	/// </summary>
	public class PagingModel
	{
		// TODO: filtering?
		public string SortBy { get; set; } = "Id";
		public bool SortDescending { get; set; }
		public int PageSize { get; set; } = 10;
		public int CurrentPage { get; set; } = 1;

		public int GetRowsToSkip() => ((CurrentPage < 1 ? 1 : CurrentPage) - 1) * PageSize;
	}
}
