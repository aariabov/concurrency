namespace Tests.My.Tasks;

public class Account
{
    private readonly object _balanceLock = new object();
    private decimal _balance;

    public Account(decimal initialBalance) => _balance = initialBalance;

    public void Debit(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "The debit amount cannot be negative.");
        }
        
        // без блокировки
        // if (_balance >= amount)
        // {
        //     _balance -= amount;
        // }

        lock (_balanceLock)
        {
            if (_balance >= amount)
            {
                _balance -= amount;
            }
        }
    }

    public void Credit(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "The credit amount cannot be negative.");
        }

        // без блокировки
        // _balance += amount;
        
        lock (_balanceLock)
        {
            _balance += amount;
        }
    }

    public decimal GetBalance()
    {
        // без блокировки
        // return _balance;
        
        lock (_balanceLock)
        {
            return _balance;
        }
    }
}