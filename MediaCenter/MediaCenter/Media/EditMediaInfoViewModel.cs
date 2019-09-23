using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Media
{
    public class EditMediaInfoViewModel : PropertyChangedNotifier
    {
        private readonly IRepository _repository;
        private readonly IStatusService _statusService;
        private readonly bool _saveChangesToRepository;

        private List<MediaItem> _items;
        private bool? _favorite;
        private bool? _private;
        private DateTime? _dateTaken;
        private string _multipleDateTaken;
        private DateTime? _dateAdded;

        private EditTagsViewModel _tagsViewModel;
        private List<string> _originalTagsIntersect;
        private bool _initInProgress;
        private string _id;
        private bool _hasMultipleItems;
        private bool _isEmpty;
        private int _itemCount;
        private Queue<Task> _saveTasks;

        public EditMediaInfoViewModel(IRepository repository, ShortcutService shortcutService, IStatusService statusService, bool saveChangesToRepository, bool readOnly = false)
        {
            _saveTasks = new Queue<Task>();

            ReadOnly = readOnly;
            _repository = repository;
            _statusService = statusService;
            _saveChangesToRepository = saveChangesToRepository;
            LoadItems(new List<MediaItem>());

            shortcutService.ToggleFavorite += (s, a) => ToggleFavorite();
        }

        public void LoadItems(List<MediaItem> items)
        {
            _items = items;
            HasMultipleItems = (items.Count > 1);
            IsEmpty = !items.Any();
            ItemCount = items.Count;

            _initInProgress = true;
            InitializeFavorite();
            InitializePrivate();
            InitializeDateTaken();
            InitializeDateAdded();
            InitializeId();
            InitializeTagsViewModel(_repository.Tags);
            _initInProgress = false;
        }

        private void StartPublishToItems()
        {
            var task = new Task(PublishToItems);
            _saveTasks.Enqueue(task);
            task.ContinueWith(t => _saveTasks.Dequeue());
            task.Start();
        }

        private void PublishToItems()
        {
            if(ReadOnly || _initInProgress)
                return;

            while (_saveTasks.Count > 1)
            {
                var waitTask = _saveTasks.Peek();
                _statusService.PostStatusMessage("Waiting for previous update operation to finish", true);
                waitTask.Wait();
            }
            _statusService.PostStatusMessage("");
            
            var total = _items.Count;
            _statusService.StartProgress();
            _statusService.PostStatusMessage($"Updating {total} items",true);

            // in a first run publish to the items, then in a second loop do the saving
            bool[] update = new bool[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                

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
                    if (HasMultipleItems)
                    {
                        // add all tags that are new
                        foreach (var newTag in TagsViewModel.SelectedTags.Where(x => !item.Tags.Contains(x)))
                        {
                            item.Tags.Add(newTag);
                            updated = true;
                        }

                        // remove tags that were in the original tags intersect and not in selected tags anymore --> has been deleted by the user
                        foreach (var deletedTag in _originalTagsIntersect.Where(x =>
                            !TagsViewModel.SelectedTags.Contains(x)))
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
                }

                update[i] = updated;
            }

            if (_saveChangesToRepository)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    _statusService.UpdateProgress(i * 100 / total);
                    if (update[i])
                    {
                        _repository.SaveItem(_items[i]).Wait();
                    }
                }
            }

            _statusService.EndProgress();
            _statusService.PostStatusMessage("");
            
        }

        public async Task Close()
        {
            while (_saveTasks.Count > 0)
            {
                var waitTask = _saveTasks.Peek();
                _statusService.PostStatusMessage("Waiting for previous update operation to finish", true);
                await waitTask;
            }
            _statusService.PostStatusMessage("");
        }

        public bool ReadOnly { get; set; }

        public int ItemCount
        {
            get => _itemCount;
            set => SetValue(ref _itemCount, value);
        }

        public bool HasMultipleItems
        {
            get => _hasMultipleItems;
            set => SetValue(ref _hasMultipleItems, value);
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetValue(ref _isEmpty, value);
        }

        public bool? Favorite
        {
            get => _favorite;
            set => SetValue(ref _favorite, value, StartPublishToItems);
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

        private void ToggleFavorite()
        {
            if(ReadOnly)
                return;

            if (Favorite == null || !Favorite.Value)
                Favorite = true;
            else
                Favorite = false;
        }

        public bool? Private
        {
            get => _private;
            set => SetValue(ref _private, value, StartPublishToItems);
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
            if (IsEmpty)
            {
                DateTaken = null;
                MultipleDateTaken = null;
            }
            else if (HasMultipleItems)
            {
                string dateFormat = "dd.MM.yyyy";
                DateTaken = null;
                var firstDate = _items.OrderBy(x => x.DateTaken).First().DateTaken.Date;
                var lastDate = _items.OrderBy(x => x.DateTaken).Last().DateTaken.Date;

                MultipleDateTaken = firstDate == lastDate 
                    ? firstDate.ToString(dateFormat) 
                    : $"{firstDate.ToString(dateFormat)} - {lastDate.ToString(dateFormat)}";
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
            DateAdded = HasMultipleItems ? null : (DateTime?)_items.FirstOrDefault()?.DateAdded;
        }

        public string Id
        {
            get => _id;
            set => SetValue(ref _id, value);
        }
        private void InitializeId()
        {
            Id = HasMultipleItems ? "" : _items.FirstOrDefault()?.Name;
        }

        public EditTagsViewModel TagsViewModel
        {
            get => _tagsViewModel;
            set => SetValue(ref _tagsViewModel, value);
        }
        private void InitializeTagsViewModel(IEnumerable<string> allTags)
        {
            // in case of multiple items, edit only the tags that are shared by all items
            var tags = HasMultipleItems
                ? (_originalTagsIntersect = _items.Select(x => x.Tags).Cast<IEnumerable<string>>().Aggregate((x, y) => x.Intersect(y)).ToList())
                : IsEmpty ? new List<string>() : _items.First().Tags.ToList();
            TagsViewModel = new EditTagsViewModel(allTags, tags);
            TagsViewModel.SelectedTags.CollectionChanged += (s, a) => StartPublishToItems();
        }



        
    }
}
