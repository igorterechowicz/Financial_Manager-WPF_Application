namespace wpf_projekt.Models
{
    public class MonthItem
    {
        public int? Number { get; }
        public string Name { get; }

        public MonthItem(int? number, string name)
        {
            Number = number;
            Name = name;
        }

        public override string ToString() => Name;
    }
}