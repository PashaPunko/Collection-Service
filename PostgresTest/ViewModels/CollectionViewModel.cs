using System.ComponentModel.DataAnnotations;
using PostgresTest.Models;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

namespace PostgresTest.ViewModels
{
    public class CollectionViewModel
    {
        public List<List<string>> optFields { get; set; } = new List<List<string>>(new List<string>[5]);
        public List<Item> Items { get; set; } = new List<Item>();
        public ProxyFilterForText FilterForName { get; set; } = new ProxyFilterForText();
        public ProxyFilterForTags FilterForTags { get; set; } = new ProxyFilterForTags();
        public List<ProxyFilterForText> FilterForText { get; set; } = new List<ProxyFilterForText>();
        public List<ProxyFilterForText> FilterForWord { get; set; } = new List<ProxyFilterForText>();
        public List<ProxyFilterForDigit> FilterForDigit { get; set; } = new List<ProxyFilterForDigit>();
        public List<ProxyFilterForDate> FilterForDate { get; set; } = new List<ProxyFilterForDate>();
        public List<ProxyFilterForCheckbox> FilterForBoolean { get; set; } = new List<ProxyFilterForCheckbox>();
        public int SortBy { get; set; }
        public int SortOrder { get; set; }
    }
    public class ProxyFilter<T>
    {
        public virtual bool Filter(T data)
        {
            return true;
        }
    }
    public class ProxyFilterForTags : ProxyFilter<List<string>>
    {
        public string FilterTags { get; set; }
        public override bool Filter(List<string> data)
        {
            if (FilterTags is null) return true;
            return data.Intersect(FilterTags.Split(' ')).Count() != 0;
        }
    }
    public class ProxyFilterForText : ProxyFilter<string>
    {
        public string FilterText { get; set; }
        public override bool Filter(string data)
        {
            if (FilterText is null) return true;
            return data.Contains(FilterText);
        }
    }
    public class ProxyFilterForDigit : ProxyFilter<int>
    {
        public int FilterDigit1 { get; set; } = Int32.MinValue;
        public int FilterDigit2 { get; set; } = Int32.MaxValue;
        public override bool Filter(int data)
        {
            return data >= FilterDigit1 && data < FilterDigit2;
        }
    }
    public class ProxyFilterForDate : ProxyFilter<DateTime>
    {
        public DateTime FilterDate1 { get; set; } = DateTime.MinValue;
        public DateTime FilterDate2 { get; set; } = DateTime.MaxValue;
        public override bool Filter(DateTime data)
        {
            return FilterDate1 <= data && FilterDate2 > data;
        }
    }

    public class ProxyFilterForCheckbox : ProxyFilter<Boolean>
    {
        public string val { get; set; } = "All";
        public override bool Filter(bool data)
        {
            if (val == "All") return true;
            return data == (val=="Yes");
        }
    }
}