using System;
using System.Collections.Generic;
using System.Linq;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Filters
{
    public class FavoriteFilter : Filter
    {
        public enum FavoriteOption { OnlyFavorite, NoFavorite, All }

        private const string NoFavorite = "No favorites";
        private const string OnlyFavorite = "Only favorites";
        private const string All = "All";

        private FavoriteOption _favoriteSetting;
        private string _favoriteString;

        public FavoriteFilter()
        {
            FavoriteSetting = FavoriteOption.OnlyFavorite;
            SetStringFromSetting(); // call manually, because if the value just set was the default value of the enum, the setter action was not executed
        }

        public static string Name = "Favorite";

        public FavoriteOption FavoriteSetting
        {
            get { return _favoriteSetting; }
            set { SetValue(ref _favoriteSetting, value, () => SetStringFromSetting()); }
        }

        public string FavoriteString
        {
            get { return _favoriteString; }
            set { SetValue(ref _favoriteString, value, () => SetSettingFromString()); }
        }

        public List<string> Options => new List<string> { OnlyFavorite, NoFavorite, All };

        private void SetSettingFromString()
        {
            switch (FavoriteString)
            {
                case NoFavorite:
                    FavoriteSetting = FavoriteOption.NoFavorite;
                    break;
                case OnlyFavorite:
                    FavoriteSetting = FavoriteOption.OnlyFavorite;
                    break;
                case All:
                    FavoriteSetting = FavoriteOption.All;
                    break;
                default:
                    FavoriteSetting = FavoriteOption.All;
                    break;
            }
        }

        private void SetStringFromSetting()
        {
            switch (FavoriteSetting)
            {
                case FavoriteOption.NoFavorite:
                    FavoriteString = NoFavorite;
                    break;
                case FavoriteOption.OnlyFavorite:
                    FavoriteString = OnlyFavorite;
                    break;
                case FavoriteOption.All:
                    FavoriteString = All;
                    break;
                default:
                    FavoriteString = "";
                    break;
            }
        }

        public override IEnumerable<MediaItem> Apply(IEnumerable<MediaItem> source)
        {
            switch (FavoriteSetting)
            {
                case FavoriteOption.OnlyFavorite:
                    return Invert ? source.Where(x => !x.Favorite) : source.Where(x => x.Favorite);
                case FavoriteOption.NoFavorite:
                    return Invert ? source.Where(x => x.Favorite) : source.Where(x => !x.Favorite);
                default:
                    return Invert ? source.Where(x => false) : source;
            }
        }
    }
}
