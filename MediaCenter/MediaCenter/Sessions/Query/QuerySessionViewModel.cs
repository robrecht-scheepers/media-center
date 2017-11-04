﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query.Filters;
using MediaCenter.Sessions.Slideshow;
using MediaCenter.Sessions.Tags;
using System.Windows;
using System.Windows.Forms;
using MediaCenter.Repository;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        private MediaItem _previousSelectedItem = null;
        
        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitializeFilterCollectionViewModel();
        }

        public override string Name => "View media";

        public QuerySession QuerySession => (QuerySession) Session;

        public IRepository Repository => Session.Repository;

        public FilterCollectionViewModel FilterCollectionViewModel { get; private set; }
        private void InitializeFilterCollectionViewModel()
        {
            FilterCollectionViewModel = new FilterCollectionViewModel(QuerySession.Filters, Repository.Tags);
        }

        public QueryResultViewModel QueryResultViewModel
        {
            get { return _queryResultViewModel; }
            set { SetValue(ref _queryResultViewModel, value); }
        }

        private void InitializeQueryResultViewModel()
        {
            if(QueryResultViewModel != null)
                QueryResultViewModel.SelectionChanged -= QueryResultViewModelOnSelectionChanged;

            QueryResultViewModel = new QueryResultDetailViewModel(QuerySession.QueryResult, Repository);
            QueryResultViewModel.SelectionChanged += QueryResultViewModelOnSelectionChanged;
        }

        private async void QueryResultViewModelOnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            EditInfoViewModel.PublishToItems();
            EditInfoViewModel = new EditInfoViewModel(args.NewSelection, Repository.Tags.ToList());
            foreach (var dirtyItem in args.OldSelection.Where(x => x.IsDirty))
            {
                await Repository.SaveItem(dirtyItem.Name);
            }
        }

        public EditInfoViewModel EditInfoViewModel
        {
            get { return _editInfoViewModel; }
            set { SetValue(ref _editInfoViewModel, value); }
        }

        #region Command: delete current selection
        private AsyncRelayCommand _deleteCurrentSelectionCommand;
        public AsyncRelayCommand DeleteCurrentSelectionCommand
            => _deleteCurrentSelectionCommand ?? (_deleteCurrentSelectionCommand = new AsyncRelayCommand(DeleteCurrentSelection, CanExecuteDeleteCurrentSelection));
        private async Task DeleteCurrentSelection()
        {
            var confirmationResult = MessageBox.Show("Are you sure you want to delete the selected items from the repository? This action cannot be undone.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (confirmationResult == MessageBoxResult.Yes)
            {
                foreach (var item in QueryResultViewModel.SelectedItems)
                {
                    await QuerySession.DeleteItem(item);
                }    
            }
        }
        private bool CanExecuteDeleteCurrentSelection()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        #endregion

        #region Command: save current selection to file
        private AsyncRelayCommand _saveCurrentSelectionToFileCommand;
        public AsyncRelayCommand SaveCurrentSelectionToFileCommand
            => _saveCurrentSelectionToFileCommand ?? (_saveCurrentSelectionToFileCommand = new AsyncRelayCommand(SaveCurrentSelectionToFile, CanExecuteSaveCurrentSelectionToFile));
        private async Task SaveCurrentSelectionToFile()
        {
            if (QueryResultViewModel == null || QueryResultViewModel.SelectedItems.Count == 0)
                return;

            if (QueryResultViewModel.SelectedItems.Count == 1)
            {
                var item = QueryResultViewModel.SelectedItems.First();
                var dialog = new SaveFileDialog()
                {
                    FileName = item.ContentFileName
                };
                dialog.ShowDialog();
                if (string.IsNullOrEmpty(dialog.FileName))
                    return;
                {
                    await Repository.SaveContentToFile(item, dialog.FileName);
                }
            }
            else
            {
                var dialog = new FolderBrowserDialog()
                {
                    Description = $"Select the destination folder for saving the selected {QueryResultViewModel.SelectedItems.Count} items"
                };
                dialog.ShowDialog();
                var selectedFolder = dialog.SelectedPath;
                if (string.IsNullOrEmpty(selectedFolder))
                    return;
                await Repository.SaveContentToFolder(QueryResultViewModel.SelectedItems.ToList(), selectedFolder);
            }
        }
        private bool CanExecuteSaveCurrentSelectionToFile()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        #endregion

        

        

        //private RelayCommand _copyTagsFromPreviousCommand;
        //public RelayCommand CopyTagsFromPreviousCommand => _copyTagsFromPreviousCommand ?? (_copyTagsFromPreviousCommand = new RelayCommand(CopyTagsFromPrevious));

        //public void CopyTagsFromPrevious()
        //{
        //    if(SelectedItem == null)
        //        return;
        //    var index = QueryResult.IndexOf(SelectedItem);
        //    if (index <= 0)
        //        return;
        //    foreach (var tag in QueryResult[index - 1].Tags)
        //    {
        //        if(!SelectedItem.Tags.Contains(tag))
        //            SelectedItem.Tags.Add(tag);
        //    }
        //    InitializeTagsViewModel();
        //}

        //public bool CanExecuteCopyTagFromPrevious()
        //{
        //    if (SelectedItem == null)
        //        return false;
        //    var index = QueryResult.IndexOf(SelectedItem);
        //    return index > 0;
        //}

        

        

        private AsyncRelayCommand _executeQueryCommand;
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
            InitializeQueryResultViewModel();

        }
        
        #region Slideshow
        private RelayCommand _startSlideShowCommand;
        public RelayCommand StartSlideShowCommand
            => _startSlideShowCommand ?? (_startSlideShowCommand = new RelayCommand(StartSlideShow));
        public void StartSlideShow()
        {
            if(SlideShowViewModel == null)
                SlideShowViewModel = new SlideShowViewModel(this);
            SlideShowActive = true;
            SlideShowViewModel.Start();
        }

        private RelayCommand _closeSlideShowCommand;
        public RelayCommand CloseSlideShowCommand => _closeSlideShowCommand ?? (_closeSlideShowCommand = new RelayCommand(CloseSlideShow));
        private void CloseSlideShow()
        {
            SlideShowViewModel.Stop();
            SlideShowActive = false;
        }

        private bool _slideShowActive;
        public bool SlideShowActive
        {
            get { return _slideShowActive; }
            set { SetValue(ref _slideShowActive, value); }
        }

        private SlideShowViewModel _slideShowViewModel;
        private QueryResultViewModel _queryResultViewModel;
        private EditInfoViewModel _editInfoViewModel;

        public SlideShowViewModel SlideShowViewModel
        {
            get { return _slideShowViewModel; }
            set { SetValue(ref _slideShowViewModel, value); }
        }
        #endregion
    }
}
