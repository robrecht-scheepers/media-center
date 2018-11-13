using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Media
{
    public class EditMediaInfoViewModel : PropertyChangedNotifier
    {
        private readonly List<MediaItem> _items;
        private readonly IRepository _repository;
        private readonly bool _saveChangesToRepository;

        private bool? _favorite;
        private bool? _private;
        private DateTime? _dateTaken;
        private string _multipleDateTaken;
        private DateTime? _dateAdded;

        private EditTagsViewModel _tagsViewModel;
        private List<string> _originalTagsIntersect;
        
        public EditMediaInfoViewModel(List<MediaItem> items, IRepository repository, bool saveChangesToRepository)
        {
            _items = items;
            _repository = repository;
            _saveChangesToRepository = saveChangesToRepository;

            if(_items == null || _items.Count == 0)
                return;

            InitializeFavorite();
            InitializePrivate();
            InitializeDateTaken();
            InitializeDateAdded();
            InitializeTagsViewModel(repository.Tags);
        }

        private void PublishToItems()
        {
            var tasks = new List<Task>();
            foreach (var item in _items)
            {
                bool updated = false;
                if (Favorite.HasValue && Favorite.Value != item.Favorite)
                {
                    item.Favorite = Favorite.Value;
                    updated = true;
                }

                if (Private.HasValue && Private.Value != item.Private)
                {
                    item.Private = Private.Value;
                    updated = true;
                }

                if (DateTaken.HasValue && DateTaken != item.DateTaken)
                {
                    item.DateTaken = DateTaken.Value;
                    updated = true;
                }

                if (TagsViewModel.IsDirty)
                {
                    if (MultipleItems)
                    {
                        // add all tags that are new
                        foreach (var newTag in TagsViewModel.SelectedTags.Where(x => !item.Tags.Contains(x)))
                        {
                            item.Tags.Add(newTag);
                            updated = true;
                        }
                        // remove tags that were in the original tags intersect and not in selected tags anymore --> has been deleted by the user
                        foreach (var deletedTag in _originalTagsIntersect.Where(x => !TagsViewModel.SelectedTags.Contains(x)))
                        {
                            item.Tags.Remove(deletedTag);
                            updated = true;
                        }
                    }
                    else
                    {
                        item.Tags.Clear();
                        foreach (var selectedTag in TagsViewModel.SelectedTags)
                        {
                            item.Tags.Add(selectedTag);
                        }
                        updated = true;
                    }
                    updated = true;
                }

                if (updated && _saveChangesToRepository)
                    tasks.Add(_repository.SaveItem(item));
            }

            foreach (var task in tasks)
            {
                task.Wait();
            }
        }

        public string Name => _items.Count == 1
            ? (_items.First().MediaType == MediaType.Video ? "Video: " : "Image: ") + _items.First().Name
            : $"{_items.Count} items selected";

        public bool MultipleItems => _items.Count > 1;

        public bool? Favorite
        {
            get => _favorite;
            set => SetValue(ref _favorite, value, PublishToItems);
        }
        private void InitializeFavorite()
        {
            if (_items.All(x => x.Favorite))
                _favorite = true;
            else if (_items.All(x => !x.Favorite))
                _favorite = false;
            else
                _favorite = null;
        }

        public bool? Private
        {
            get => _private;
            set => SetValue(ref _private, value, PublishToItems);
        }
        private void InitializePrivate()
        {
            if (_items.All(x => x.Private))
                _private = true;
            else if (_items.All(x => !x.Private))
                _private = false;
            else
                _private = null;
        }

        public DateTime? DateTaken
        {
            get => _dateTaken;
            set => SetValue(ref _dateTaken, value);
        }

        public string MultipleDateTaken
        {
            get => _multipleDateTaken;
            set => SetValue(ref _multipleDateTaken, value);
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
            get => _dateAdded;
            private set => SetValue(ref _dateAdded, value);
        }
        private void InitializeDateAdded()
        {
            DateAdded = MultipleItems ? null : (DateTime?)_items.First().DateAdded;
        }

        public EditTagsViewModel TagsViewModel
        {
            get => _tagsViewModel;
            set => SetValue(ref _tagsViewModel, value);
        }
        private void InitializeTagsViewModel(IEnumerable<string> allTags)
        {
            // in case of multiple items, edit only the tags that are shared by all items
            var tags = MultipleItems
                ? (_originalTagsIntersect = _items.Select(x => x.Tags).Cast<IEnumerable<string>>().Aggregate((x, y) => x.Intersect(y)).ToList())
                : _items.First().Tags.ToList();
            TagsViewModel = new EditTagsViewModel(allTags, tags);
            TagsViewModel.SelectedTags.CollectionChanged += (s, a) => PublishToItems();
        }

        
    }
}
