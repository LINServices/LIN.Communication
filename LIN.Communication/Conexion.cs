namespace LIN.Communication;


/// <summary>
/// Conexión con la base de datos
/// </summary>
public sealed class Conexión
{


    //===== Estáticas =====//

    /// <summary>
    /// String de Conexión
    /// </summary>
    private static string _connection = string.Empty;


    /// <summary>
    /// Contador de conexiones abiertas
    /// </summary>
    private static volatile int _counter = 0;


    /// <summary>
    /// Cantidad de conexiones que se pueden almacenar en cache
    /// </summary>
    private static int _cantidad = 1;



    //===== Propiedades =====//


    /// <summary>
    /// Obtiene o establece si la Conexión esta en uso
    /// </summary>
    private volatile bool OnUse = false;



    /// <summary>
    /// Obtiene si la Conexión esta en uso y la pone en uso
    /// </summary>
    private bool OnUseAction
    {

        get
        {
            lock (this)
            {
                if (!OnUse)
                {
                    OnUse = true;
                    return false;
                }

                return true;

            }

        }

    }



    /// <summary>
    /// Obtiene el numero de Conexión
    /// </summary>
    public readonly int ConnectionNumber;



    /// <summary>
    /// Cache de conexiones
    /// </summary>
    private static List<Conexión> CacheConnections { get; set; } = new();






    /// <summary>
    /// Obtiene la base de datos
    /// </summary>
    public Data.Context DataBase { get; private set; }


  

    /// <summary>
    /// Nueva Conexión
    /// </summary>
    private Conexión()
    {

        DbContextOptionsBuilder<Data.Context> optionsBuilder = new();
        optionsBuilder.UseSqlServer(_connection);

        DataBase = new Data.Context(optionsBuilder.Options);

        _counter++;
        ConnectionNumber = _counter;

        if (CacheConnections.Count <= _cantidad)
            CacheConnections.Add(this);

    }



    /// <summary>
    /// Destructor
    /// </summary>
    ~Conexión()
    {
        this?.DataBase?.Dispose();
    }




    /// <summary>
    /// Establece que la conexión esta en uso
    /// </summary>
    public void SetOnUse()
    {
        lock (this)
        {
            OnUse = true;
        }

    }



    private string mykey = string.Empty;

    public void CloseActions(string key)
    {
        lock (this)
        {
            if (mykey != key)
                return;

            DataBase.ChangeTracker.Clear();
            mykey = string.Empty;
            OnUse = false;
        }
    }



    /// <summary>
    /// Inicia las conexiones del cache
    /// </summary>
    public static async Task StartConnections()
    {

        _cantidad = 5;
#if AZURE
        _cantidad = 30;
#elif SOMEE
        _cantidad = 10;
#elif DEBUG
        _cantidad = 50;
#endif

        await Task.Run(() =>
          {
              for (var i = 0; i < _cantidad; i++)
              {
                  _ = new Conexión();
              }
          });

    }



    /// <summary>
    /// Establece el string de Conexión
    /// </summary>
    /// <param name="connectionString">string de Conexión</param>
    public static void SetStringConnection(string connectionString)
    {
        _connection = connectionString;
    }



    /// <summary>
    /// Obtiene una Conexión a la base de datos
    /// </summary>
    public static (Conexión context, string contextKey) GetOneConnection()
    {

        // Obtiene una Conexión de la pool
        var con = CacheConnections.FirstOrDefault(T => !T.OnUseAction);

        if (con != null && con.mykey == string.Empty)
        {
            lock (con)
            {
                con.SetOnUse();
                string key = KeyGen.Generate(10, "con.");
                con.mykey = key;
                return (con, key);
            }
        }

        // Retorna la Conexión
        var conexión = new Conexión
        {
            mykey = KeyGen.Generate(10, "con.")
        };
        conexión.SetOnUse();
        return (conexión, conexión.mykey);

    }



    /// <summary>
    /// Obtiene una Conexión alterna a la base de datos
    /// </summary>
    public static Conexión GetForcedConnection(string? message = null)
    {
        return new();
    }


}