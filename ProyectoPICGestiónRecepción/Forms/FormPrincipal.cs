using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoPICGestiónRecepción.Forms
{
    public partial class FormPrincipal : Form
    {
        public FormPrincipal()
        {
            InitializeComponent();
        }

        private void clienteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbrirFormularioHijo(typeof(FormCliente));
        }

        private void vehiculoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbrirFormularioHijo(typeof(FormVehiculo));
        }

        private void representanteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbrirFormularioHijo(typeof(FormRepresentante));
        }

        private void proveedorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbrirFormularioHijo(typeof(FormProveedor));
        }

        private void responsableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbrirFormularioHijo(typeof(FormResponsableEstiba));
        }

        private void registroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AbrirFormularioHijo(typeof(FormRegistroEstiba));
        }

        // Método general para abrir formularios hijos
        private void AbrirFormularioHijo(Type tipoFormulario)
        {
            foreach (Form f in this.MdiChildren)
            {
                if (f.GetType() == tipoFormulario)
                {
                    f.Activate();
                    return;
                }
            }

            Form formulario = (Form)Activator.CreateInstance(tipoFormulario);
            formulario.MdiParent = this;
            formulario.Show();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
