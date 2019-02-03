﻿using System;
using System.Linq;

namespace TASVideos.Data.Entity
{
    public interface ITrackable
    {
		DateTime CreateTimeStamp { get; set; }
		string CreateUserName { get; set; }

		DateTime LastUpdateTimeStamp { get; set; }
		string LastUpdateUserName { get; set; }
	}

	public class BaseEntity : ITrackable
	{
		public DateTime CreateTimeStamp { get; set; }
		public string CreateUserName { get; set; }

		public DateTime LastUpdateTimeStamp { get; set; }
		public string LastUpdateUserName { get; set; }
	}

	public static class TrackableQueryableExtensions
	{
		public static IQueryable<T> CreatedBy<T>(this IQueryable<T> list, string userName) 
			where T : ITrackable
		{
			return list.Where(t => t.CreateUserName == userName);
		}

		public static IQueryable<T> OldestToNewest<T>(this IQueryable<T> list)
			where T : ITrackable
		{
			return list.OrderBy(t => t.CreateTimeStamp);
		}

		public static IQueryable<T> ByMostRecent<T>(this IQueryable<T> list)
			where T : ITrackable
		{
			return list.OrderByDescending(t => t.CreateTimeStamp);
		}

		public static IQueryable<T> Since<T>(this IQueryable<T> list, DateTime target)
			where T: ITrackable
		{
			return list.Where(t => t.CreateTimeStamp >= target);
		}
	}
}
