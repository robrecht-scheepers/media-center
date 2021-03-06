﻿using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Filters
{
    public class PrivateFilter : Filter
    {
        public enum PrivateOption { NoPrivate, OnlyPrivate, All}

        private const string NoPrivate = "No private";
        private const string PrivateOnly  = "Only private";
        private const string All = "All";
        

        private string _privateString;
        private PrivateOption _privateSetting;

        public PrivateFilter()
        {
            PrivateSetting = PrivateOption.NoPrivate;
            SetStringFromSetting(); // call manually, because if the value just set was the default value of the enum, the setter action was not executed
        }

        public static string Name = "Private";

        public override IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source)
        {
            switch(PrivateSetting)
            {
                case PrivateOption.NoPrivate:
                    return Invert ? source.Where(x => x.Private) : source.Where(x => !x.Private);
                case PrivateOption.OnlyPrivate:
                    return Invert ? source.Where(x => !x.Private) : source.Where(x => x.Private);
                case PrivateOption.All:
                    return Invert ? source.Where(x => false) : source;
                default:
                    throw new ArgumentException("PrivateSetting");
            }
        }

        public string PrivateString
        {
            get { return _privateString; }
            set { SetValue(ref _privateString, value, () => SetSettingFromString()); }
        }

        public PrivateOption PrivateSetting
        {
            get { return _privateSetting; }
            set { SetValue(ref _privateSetting, value, () => SetStringFromSetting()); }
        }

        public List<string> Options => new List<string> { NoPrivate, PrivateOnly, All};

        private void SetSettingFromString()
        {
            switch (PrivateString)
            {
                case NoPrivate:
                    PrivateSetting = PrivateOption.NoPrivate;
                    break;
                case PrivateOnly:
                    PrivateSetting = PrivateOption.OnlyPrivate;
                    break;
                case All:
                    PrivateSetting = PrivateOption.All;
                    break;
                default:
                    PrivateSetting = PrivateOption.NoPrivate;
                    break;
            }
        }

        private void SetStringFromSetting()
        {
            switch(PrivateSetting)
            {
                case PrivateOption.NoPrivate:
                    PrivateString = NoPrivate;
                    break;
                case PrivateOption.OnlyPrivate:
                    PrivateString = PrivateOnly;
                    break;
                case PrivateOption.All:
                    PrivateString = All;
                    break;
            }
        }
    }
}
