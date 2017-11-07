﻿using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Filters
{
    public class TagFilter : Filter
    {
        public static string Name = "Tag";

        private string _tag;

        public TagFilter(IEnumerable<string> tags)
        {
            Tags = tags?.ToList();
        }

        public string Tag
        {
            get { return _tag; }
            set { SetValue(ref _tag, value); }
        }

        public List<string> Tags { get; }

        public override IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source)
        {
            if (string.IsNullOrEmpty(Tag))
                return source;

            switch (FilterMode)
            {
                case FilterMode.Match:
                    return source.Where(x => x.Tags.Contains(Tag));
                case FilterMode.NoMatch:
                    return source.Where(x => !x.Tags.Contains(Tag));
                default:
                    throw new ArgumentOutOfRangeException(nameof(FilterMode), FilterMode, null);
            }
        }
    }
}