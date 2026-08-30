using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;

namespace CeyPASS.WPF;

/// <summary>Grid / Editor / SearchPanel kullanıcı metinleri (TR).</summary>
internal sealed class CeypassGridLocalizer : GridControlLocalizer
{
    public override string GetLocalizedString(GridControlStringId id) => id switch
    {
        GridControlStringId.MenuColumnSortAscending => "Artan sırala",
        GridControlStringId.MenuColumnSortDescending => "Azalan sırala",
        GridControlStringId.MenuColumnClearSorting => "Sıralamayı temizle",
        GridControlStringId.MenuColumnGroup => "Bu sütuna göre grupla",
        GridControlStringId.MenuColumnUnGroup => "Gruplamayı kaldır",
        GridControlStringId.MenuColumnShowGroupPanel => "Grup panelini göster",
        GridControlStringId.MenuColumnHideGroupPanel => "Grup panelini gizle",
        GridControlStringId.MenuColumnShowColumnChooser => "Sütun seçiciyi göster",
        GridControlStringId.MenuColumnHideColumnChooser => "Sütun seçiciyi gizle",
        GridControlStringId.MenuColumnShowColumnBandChooser => "Sütun/bant seçiciyi göster",
        GridControlStringId.MenuColumnHideColumnBandChooser => "Sütun/bant seçiciyi gizle",
        GridControlStringId.MenuColumnBestFit => "En uygun genişlik",
        GridControlStringId.MenuColumnBestFitColumns => "En uygun genişlik (tüm sütunlar)",
        GridControlStringId.MenuColumnFilterEditor => "Filtre düzenleyici...",
        GridControlStringId.MenuColumnClearFilter => "Filtreyi temizle",
        GridControlStringId.MenuColumnShowSearchPanel => "Arama panelini göster",
        GridControlStringId.MenuColumnHideSearchPanel => "Arama panelini gizle",
        GridControlStringId.MenuColumnFixedStyle => "Sabitleme",
        GridControlStringId.MenuColumnFixedLeft => "Sola sabitle",
        GridControlStringId.MenuColumnFixedRight => "Sağa sabitle",
        GridControlStringId.MenuColumnFixedNone => "Sabitleme yok",

        GridControlStringId.AutoFilterNullText => "Filtrele...",
        GridControlStringId.FilterEditorTitle => "Filtre düzenleyici",
        GridControlStringId.GridGroupPanelText => "Gruplamak için bir sütun başlığını buraya sürükleyin",
        GridControlStringId.ColumnChooserCaption => "Sütun seçici",
        GridControlStringId.ColumnChooserDragText => "Gizlemek için bir sütun başlığını buraya sürükleyin",

        GridControlStringId.PopupFilterAll => "(Tümü)",
        GridControlStringId.PopupFilterBlanks => "(Boşlar)",
        GridControlStringId.PopupFilterNonBlanks => "(Dolu olanlar)",

        GridControlStringId.ExcelColumnFilterPopupClearFilter => "Filtreyi temizle",
        GridControlStringId.ExcelColumnFilterPopupValuesTabCaption => "FİLTRE DEĞERLERİ",
        GridControlStringId.ExcelColumnFilterPopupFilterRulesTabCaption => "FİLTRE KURALLARI",
        GridControlStringId.ExcelColumnFilterPopupSearchNullText => "Ara",
        GridControlStringId.ExcelColumnFilterPopupSearchNullTextAll => "Ara",
        GridControlStringId.ExcelColumnFilterPopupSearchNullTextDate => "Tarih ara",
        GridControlStringId.ExcelColumnFilterPopupSearchNullTextMonth => "Ay ara",
        GridControlStringId.ExcelColumnFilterPopupSearchNullTextYear => "Yıl ara",
        GridControlStringId.ExcelColumnFilterPopupEnterValue => "Değer girin",
        GridControlStringId.ExcelColumnFilterPopupFilterBetweenFrom => "Başlangıç",
        GridControlStringId.ExcelColumnFilterPopupFilterBetweenTo => "Bitiş",
        GridControlStringId.ExcelColumnFilterPopupSelectDate => "Tarih seç",
        GridControlStringId.ExcelColumnFilterPopupSelectValue => "Değer seç",
        GridControlStringId.ExcelColumnFilterPopupSearchScopeAll => "Tümü",
        GridControlStringId.ExcelColumnFilterPopupSearchScopeDay => "Gün",
        GridControlStringId.ExcelColumnFilterPopupSearchScopeMonth => "Ay",
        GridControlStringId.ExcelColumnFilterPopupSearchScopeYear => "Yıl",

        GridControlStringId.MenuGroupPanelFullExpand => "Tümünü genişlet",
        GridControlStringId.MenuGroupPanelFullCollapse => "Tümünü daralt",
        GridControlStringId.MenuGroupPanelClearGrouping => "Gruplamayı temizle",

        _ => base.GetLocalizedString(id)
    };
}

internal sealed class CeypassEditorLocalizer : EditorLocalizer
{
    public override string GetLocalizedString(EditorStringId id) => id switch
    {
        EditorStringId.OK => "Tamam",
        EditorStringId.Cancel => "İptal",
        EditorStringId.Apply => "Uygula",
        EditorStringId.Today => "Bugün",
        EditorStringId.Clear => "Temizle",

        EditorStringId.FilterGroupAnd => "Ve",
        EditorStringId.FilterGroupOr => "Veya",
        EditorStringId.FilterGroupNotAnd => "Ve değil",
        EditorStringId.FilterGroupNotOr => "Veya değil",
        EditorStringId.FilterGroupNotAndMenuCaption => "Ve değil",
        EditorStringId.FilterGroupNotOrMenuCaption => "Veya değil",
        EditorStringId.FilterGroupAddCondition => "Koşul ekle",
        EditorStringId.FilterGroupAddGroup => "Grup ekle",
        EditorStringId.FilterGroupAddCustomExpression => "Özel ifade ekle",
        EditorStringId.FilterGroupClearAll => "Tümünü temizle",
        EditorStringId.FilterGroupRemoveGroup => "Grubu kaldır",
        EditorStringId.FilterCriteriaToStringGroupOperatorAnd => "Ve",
        EditorStringId.FilterCriteriaToStringGroupOperatorOr => "Veya",

        EditorStringId.FilterClauseEquals => "Eşittir",
        EditorStringId.FilterClauseDoesNotEqual => "Eşit değildir",
        EditorStringId.FilterClauseGreater => "Büyüktür",
        EditorStringId.FilterClauseGreaterOrEqual => "Büyük veya eşittir",
        EditorStringId.FilterClauseLess => "Küçüktür",
        EditorStringId.FilterClauseLessOrEqual => "Küçük veya eşittir",
        EditorStringId.FilterClauseBetween => "Arasında",
        EditorStringId.FilterClauseBetweenAnd => "ve",
        EditorStringId.FilterClauseNotBetween => "Arasında değil",
        EditorStringId.FilterClauseContains => "İçerir",
        EditorStringId.FilterClauseDoesNotContain => "İçermez",
        EditorStringId.FilterClauseBeginsWith => "İle başlar",
        EditorStringId.FilterClauseEndsWith => "İle biter",
        EditorStringId.FilterClauseLike => "Benzer",
        EditorStringId.FilterClauseNotLike => "Benzer değil",
        EditorStringId.FilterClauseIsNull => "Boş",
        EditorStringId.FilterClauseIsNotNull => "Boş değil",
        EditorStringId.FilterClauseIsNullOrEmpty => "Boş veya yok",
        EditorStringId.FilterClauseIsNotNullOrEmpty => "Boş veya yok değil",
        EditorStringId.FilterClauseAnyOf => "Şunlardan biri",
        EditorStringId.FilterClauseNoneOf => "Hiçbiri",
        EditorStringId.FilterClauseIsToday => "Bugün",
        EditorStringId.FilterClauseIsYesterday => "Dün",
        EditorStringId.FilterClauseIsTomorrow => "Yarın",
        EditorStringId.FilterClauseLocalDateTimeToday => "Bugün",
        EditorStringId.FilterClauseLocalDateTimeYesterday => "Dün",
        EditorStringId.FilterClauseLocalDateTimeTomorrow => "Yarın",
        EditorStringId.FilterClauseLocalDateTimeNow => "Şimdi",
        EditorStringId.FilterClauseLocalDateTimeThisWeek => "Bu hafta",
        EditorStringId.FilterClauseLocalDateTimeThisMonth => "Bu ay",
        EditorStringId.FilterClauseLocalDateTimeThisYear => "Bu yıl",

        EditorStringId.FilterEditorChecked => "Seçili",
        EditorStringId.FilterEditorUnchecked => "Seçili değil",
        EditorStringId.FilterPanelClearFilter => "Filtreyi temizle",
        EditorStringId.FilterPanelEditFilter => "Filtreyi düzenle",
        EditorStringId.FilterPanelEnableFilter => "Filtreyi aç",
        EditorStringId.FilterPanelDisableFilter => "Filtreyi kapat",
        EditorStringId.FilterPanelExpand => "Genişlet",
        EditorStringId.FilterPanelCollapse => "Daralt",

        _ => base.GetLocalizedString(id)
    };
}

internal sealed class CeypassSearchPanelLocalizer : SearchPanelLocalizer
{
    public override string GetLocalizedString(SearchPanelStringId id) => id switch
    {
        SearchPanelStringId.LabelText_Find => "Bul:",
        SearchPanelStringId.LabelText_Replace => "Değiştir:",
        SearchPanelStringId.ButtonTooltip_FindNext => "Sonrakini bul",
        SearchPanelStringId.ButtonTooltip_FindPrev => "Öncekini bul",
        SearchPanelStringId.ButtonTooltip_Close => "Kapat",
        SearchPanelStringId.ButtonTooltip_SearchOptions => "Arama seçenekleri",
        SearchPanelStringId.ButtonText_Replace => "Değiştir",
        SearchPanelStringId.ButtonText_ReplaceAll => "Tümünü değiştir",
        SearchPanelStringId.MenuCheckItem_CaseSensative => "Büyük/küçük harf duyarlı",
        SearchPanelStringId.MenuCheckItem_WholeWord => "Sözcüğün tamamı",
        SearchPanelStringId.MenuCheckItem_UseRegularExpression => "Düzenli ifade",
        _ => base.GetLocalizedString(id)
    };
}
