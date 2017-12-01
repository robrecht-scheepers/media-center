﻿using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.MVVM;

namespace MediaCenter.Media
{
    public class EditMediaInfoViewModel : PropertyChangedNotifier
    {
        private List<MediaItem> _items;
        
        private bool? _favorite;
        private bool? _private;
        private DateTime? _dateTaken;
        private string _multipleDateTaken;
        private DateTime? _dateAdded;

        private EditTagsViewModel _tagsViewModel;
        private List<string> _originalTagsIntersect;
        private string _name;

        public EditMediaInfoViewModel(List<MediaItem> items, List<string> allTags)
        {
            _items = items;
            
            if(_items == null || _items.Count == 0)
                return;

            InitializeFavorite();
            InitializePrivate();
            InitializeDateTaken();
            InitializeDateAdded();
            InitializeTagsViewModel(allTags);
        }

        public void PublishToItems()
        {
            foreach (var item in _items)
            {
                if (Favorite.HasValue && Favorite.Value != item.Favorite)
                {
                    item.Favorite = Favorite.Value;
                    item.IsInfoDirty = true;
                }

                if (Private.HasValue && Private.Value != item.Private)
                {
                    item.Private = Private.Value;
                    item.IsInfoDirty = true;
                }

                if (DateTaken.HasValue && DateTaken != item.DateTaken)
                {
                    item.DateTaken = DateTaken.Value;
                    item.IsInfoDirty = true;
                }

                if (TagsViewModel.IsDirty)
                {
                    if (MultipleItems)
                    {
                        // add all tags that are new
                        foreach (var newTag in TagsViewModel.SelectedTags.Where(x => !item.Tags.Contains(x)))
                        {
                            item.Tags.Add(newTag);
                        }
                        // remove tags that were in the original tags intersect and not in selected tags anymore --> has been deleted by the user
                        foreach (var deletedTag in _originalTagsIntersect.Where(x => !TagsViewModel.SelectedTags.Contains(x)))
                        {
                            item.Tags.Remove(deletedTag);
                        }
                    }
                    else
                    {
                        item.Tags.Clear();
                        foreach (var selectedTag in TagsViewModel.SelectedTags)
                        {
                            item.Tags.Add(selectedTag);
                        }
                    }
                    item.IsInfoDirty = true;
                }
            }
        }

        public string Name => _items.Count == 1
            ? (_items.First().MediaType == MediaType.Video ? "Video: " : "Image: ") + _items.First().Name
            : $"{_items.Count} items selected";

        public bool MultipleItems => _items.Count > 1;

        public bool? Favorite
        {
            get { return _favorite; }
            set { SetValue(ref _favorite, value); }
        }
        private void InitializeFavorite()
        {
            if (_items.All(x => x.Favorite))
                Favorite = true;
            else if (_items.All(x => !x.Favorite))
                Favorite = false;
            else
                Favorite = null;
        }

        public bool? Private
        {
            get { return _private; }
            set { SetValue(ref _private, value); }
        }
        private void InitializePrivate()
        {
            if (_items.All(x => x.Private))
                Private = true;
            else if (_items.All(x => !x.Private))
                Private = false;
            else
                Private = null;
        }

        public DateTime? DateTaken
        {
            get { return _dateTaken; }
            set { SetValue(ref _dateTaken, value); }
        }

        public string MultipleDateTaken
        {
            get { return _multipleDateTaken; }
            set { SetValue(ref _multipleDateTaken, value); }
        }
        private void InitializeDateTaken()
        {
            if (MultipleItems)
            {
                string dateFormat = "dd.MM.yyyy";
                DateTaken = null;
                var firstDate = _items.OrderBy(x => x.DateTaken).First().DateTaken.Date;
                var lastDate = _items.OrderBy(x => x.DateTaken).Last().DateTaken.Date;

                if (firstDate == lastDate)
                    MultipleDateTaken = firstDate.ToString(dateFormat);
                else
                    MultipleDateTaken = $"{firstDate.ToString(dateFormat)} - {lastDate.ToString(dateFormat)}";
            }
            else
            {
                DateTaken = (DateTime?)_items.First().DateTaken;
                MultipleDateTaken = null;
            }
            
            
            
        }

        public DateTime? DateAdded
        {
            get { return _dateAdded; }
            set { SetValue(ref _dateAdded, value); }
        }
        private void InitializeDateAdded()
        {
            DateAdded = MultipleItems ? null : (DateTime?)_items.First().DateAdded;
        }

        public EditTagsViewModel TagsViewModel
        {
            get { return _tagsViewModel; }
            set { SetValue(ref _tagsViewModel, value); }
        }
        private void InitializeTagsViewModel(IEnumerable<string> allTags)
        {
            // in case of multiple items, edit only the tags that are shared by all items
            var tags = MultipleItems
                ? (_originalTagsIntersect = _items.Select(x => x.Tags).Cast<IEnumerable<string>>().Aggregate((x, y) => x.Intersect(y)).ToList())
                : _items.First().Tags.ToList();
            TagsViewModel = new EditTagsViewModel(allTags, tags);
        }
    }
}
