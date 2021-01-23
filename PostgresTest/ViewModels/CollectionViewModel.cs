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
        public ProxyFilterForText FilterForName = new ProxyFilterForText();
        public ProxyFilterForTags FilterForTags { get; set; } = new ProxyFilterForTags();
        public List<ProxyFilterForText> FilterForText { get; set; } = new List<ProxyFilterForText>();
        public List<ProxyFilterForText> FilterForWord { get; set; } = new List<ProxyFilterForText>();
        public List<ProxyFilterForDigit> FilterForDigit { get; set; } = new List<ProxyFilterForDigit>();
        public List<ProxyFilterForDate> FilterForDate { get; set; } = new List<ProxyFilterForDate>();
        public List<ProxyFilterForCheckbox> FilterForBoolean { get; set; } = new List<ProxyFilterForCheckbox>();
        public KeyValuePair<int, bool> SortASC;
    }
    public class ProxyFilter<T>
    {
        public ProxyFilter()
        {
        }
        public virtual bool Filter(T data)
        {
            return true;
        }
    }
    public class ProxyFilterForTags : ProxyFilter<List<string>>
    {
        public string FilterTags;
        public override bool Filter(List<string> data)
        {
            if (FilterTags is null) return true;
            return data.Intersect(FilterTags.Split(' ')).Count() != 0;
        }
    }
    public class ProxyFilterForText : ProxyFilter<string>
    {
        public string FilterText;
        public override bool Filter(string data)
        {
            if (FilterText is null) return true;
            return data.Contains(FilterText);
        }
    }
    public class ProxyFilterForDigit : ProxyFilter<int>
    {
        public int FilterDigit1 = Int32.MinValue;
        public int FilterDigit2 = Int32.MaxValue;
        public ProxyFilterForDigit()
        {
            this.FilterDigit1 = Int32.MinValue;
            this.FilterDigit2 = Int32.MaxValue;
        }
        public override bool Filter(int data)
        {
            return data >= FilterDigit1 && data < FilterDigit2;
        }
    }
    public class ProxyFilterForDate : ProxyFilter<DateTime>
    {
        public DateTime FilterDate1 = DateTime.MinValue;
        public DateTime FilterDate2 = DateTime.MaxValue;
        public override bool Filter(DateTime data)
        {
            return FilterDate1 <= data && FilterDate2 > data;
        }
    }
    public class Bool
    {
        public bool data;
    }
    public class ProxyFilterForCheckbox : ProxyFilter<Boolean>
    {

        public Bool FilterBoolean;
        public override bool Filter(bool data)
        {
            if (FilterBoolean is null) return true;
            return data == FilterBoolean.data;
        }
    }
}