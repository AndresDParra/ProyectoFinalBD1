using System;
using System.Windows.Forms;

namespace GestionVentasPuro
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Crear base de datos local y tablas requeridas al iniciar
            Database.InicializarBD();

            Application.Run(new FormLogin());
        }
    }
}
