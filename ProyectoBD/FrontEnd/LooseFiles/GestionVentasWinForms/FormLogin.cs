using System.Windows.Forms;

namespace GestionVentasPuro
{
    public class FormLogin : Form
    {
        public FormLogin()
        {
            Text = "Login";
            Width = 360;
            Height = 220;
            StartPosition = FormStartPosition.CenterScreen;

            var label = new Label
            {
                Text = "Inicio de sesión (stub)",
                AutoSize = true,
                Left = 20,
                Top = 20
            };
            Controls.Add(label);
        }
    }
}
