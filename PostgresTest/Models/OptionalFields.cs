using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NpgsqlTypes;

namespace UserCollections.Models
{
    public class IField<T> {
        public int Id { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
        public virtual T GetField() {
            return default(T);
        }
    }
    public class Tag :IField<string>
    {
        public string Text { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public override string GetField()
        {
            return Text;
        }
    }
    public class TextField : IField<string>
    {
        public string Text { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public override string GetField()
        {
            return Text;
        }
    }
    public class DigitField : IField<int>
    {
        public int Digit { get; set; }
        public override int GetField()
        {
            return Digit;
        }
    }
    public class WordField : IField<string>
    {
        public string Word { get; set; }
        public NpgsqlTsVector SearchVector { get; set; }
        public override string GetField()
        {
            return Word;
        }
    }
    public class DateField:IField<DateTime>
    {
        public DateTime Date { get; set; }
        public override DateTime GetField()
        {
            return Date;
        }
    }
    public class CheckboxField : IField<bool>
    {
        public bool Checkbox { get; set; }
        public override bool GetField()
        {
            return Checkbox;
        }
    }
    
}
