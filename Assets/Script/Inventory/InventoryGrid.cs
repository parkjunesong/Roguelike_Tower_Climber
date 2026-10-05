using System;

// All item types share the same slots. Each item instance occupies one slot.
public class InventoryGrid
{
    private readonly ItemInstance[] slots;

    public int Columns { get; }
    public int Rows { get; }
    public int Capacity => slots.Length;
    public int Count { get; private set; }
    public event Action Changed;

    public InventoryGrid(int columns, int rows)
    {
        if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
        if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
        Columns = columns;
        Rows = rows;
        slots = new ItemInstance[checked(columns * rows)];
    }

    public ItemInstance GetItem(int index)
    {
        if (index < 0 || index >= Capacity) throw new ArgumentOutOfRangeException(nameof(index));
        return slots[index];
    }

    public ItemInstance GetItem(int column, int row)
    {
        if (column < 0 || column >= Columns) throw new ArgumentOutOfRangeException(nameof(column));
        if (row < 0 || row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
        return slots[row * Columns + column];
    }

    public bool TryAdd(ItemInstance item)
    {
        if (item == null || Count == Capacity || Array.IndexOf(slots, item) >= 0) return false;
        int index = Array.IndexOf(slots, null);
        slots[index] = item;
        Count++;
        Changed?.Invoke();
        return true;
    }

    public bool TryRemove(int index)
    {
        if (index < 0 || index >= Capacity || slots[index] == null) return false;
        slots[index] = null;
        Count--;
        Changed?.Invoke();
        return true;
    }

    public int IndexOf(ItemInstance item) => item == null ? -1 : Array.IndexOf(slots, item);

    public void SortByType()
    {
        Array.Sort(slots, (a, b) =>
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            int type = StringComparer.Ordinal.Compare(a.ItemType, b.ItemType);
            return type != 0 ? type : StringComparer.Ordinal.Compare(a.DisplayName, b.DisplayName);
        });
        Changed?.Invoke();
    }
}
