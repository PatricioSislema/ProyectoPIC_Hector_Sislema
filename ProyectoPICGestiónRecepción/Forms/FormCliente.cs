using ProyectoPICGestiónRecepción.Data;
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
    public partial class FormCliente : Form
    {
        private int? _idSeleccionado = null;

        public FormCliente()
        {
            InitializeComponent();
        }

        private void FormCliente_Load(object sender, EventArgs e)
        {
            CargarDatos();
            LimpiarFormulario();
            ConfigurarAutoCompleteNombre();
        }
        private void ConfigurarAutoCompleteNombre()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var nombres = db.Clientes
                        .Select(c => c.Nombre.ToUpper())
                        .ToArray();

                    var autoComplete = new AutoCompleteStringCollection();
                    autoComplete.AddRange(nombres);

                    txtConsultaCliente.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtConsultaCliente.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtConsultaCliente.AutoCompleteCustomSource = autoComplete;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al configurar autocompletado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarDatos()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    dgvClientes.DataSource = db.Clientes
                        .OrderBy(c => c.Nombre)
                        .Select(c => new
                        {
                            c.IdCliente,
                            c.Nombre,
                            c.RUC,
                            c.Direccion,
                            c.Telefono,
                            c.Email
                        })
                        .ToList();
                    // Cambiar encabezado de IdCliente
                    if (dgvClientes.Columns["IdCliente"] != null)
                        dgvClientes.Columns["IdCliente"].HeaderText = "Código";

                    // Ajustar columnas al contenido
                    dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                }
                dgvClientes.ClearSelection();
                _idSeleccionado = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtRUC.Text = "";
            txtDireccion.Text = "";
            txtTelefono.Text = "";
            txtEmail.Text = "";
            txtConsultaCliente.Text = "";
            errorProvider1.Clear();
            _idSeleccionado = null;
        }

        private void LimpiarYEnfocar(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.Clear();
            textBox.Focus();
        }

        private bool ValidarFormulario()
        {
            bool ok = true;
            errorProvider1.Clear();

            // Nombre obligatorio
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errorProvider1.SetError(txtNombre, "Nombre es obligatorio");
                LimpiarYEnfocar(txtNombre);
                ok = false;
                return ok; // corta validación al primer error
            }

            // RUC obligatorio y solo números, exactamente 13 dígitos
            if (string.IsNullOrWhiteSpace(txtRUC.Text))
            {
                errorProvider1.SetError(txtRUC, "RUC es obligatorio");
                LimpiarYEnfocar(txtRUC);
                ok = false;
                return ok;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(txtRUC.Text, @"^\d{13}$"))
            {
                errorProvider1.SetError(txtRUC, "RUC inválido: debe contener 13 números");
                LimpiarYEnfocar(txtRUC);
                ok = false;
                return ok;
            }

            // Dirección obligatoria
            if (string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                errorProvider1.SetError(txtDireccion, "Dirección es obligatoria");
                LimpiarYEnfocar(txtDireccion);
                ok = false;
                return ok;
            }

            // Teléfono obligatorio, solo números y máximo 10 dígitos
            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                errorProvider1.SetError(txtTelefono, "Teléfono es obligatorio");
                LimpiarYEnfocar(txtTelefono);
                ok = false;
                return ok;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(txtTelefono.Text, @"^\d{1,10}$"))
            {
                errorProvider1.SetError(txtTelefono, "Teléfono inválido: solo números y máximo 10 dígitos");
                LimpiarYEnfocar(txtTelefono);
                ok = false;
                return ok;
            }

            // Email opcional pero validar formato si ingresa algo
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    errorProvider1.SetError(txtEmail, "Email inválido: ingrese un correo válido");
                    LimpiarYEnfocar(txtEmail);
                    ok = false;
                    return ok;
                }
            }

            return ok;
        }




        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    if (_idSeleccionado == null)
                    {
                        var cli = new Cliente
                        {
                            Nombre = txtNombre.Text.Trim().ToUpper(),
                            RUC = txtRUC.Text.Trim().ToUpper(),
                            Direccion = txtDireccion.Text.Trim().ToUpper(),
                            Telefono = txtTelefono.Text.Trim().ToUpper(),
                            Email = txtEmail.Text.Trim()
                        };
                        db.Clientes.Add(cli);
                    }
                    else
                    {
                        var cli = db.Clientes.Find(_idSeleccionado.Value);
                        if (cli == null) return;
                        cli.Nombre = txtNombre.Text.Trim().ToUpper();
                        cli.RUC = txtRUC.Text.Trim().ToUpper();
                        cli.Direccion = txtDireccion.Text.Trim().ToUpper();
                        cli.Telefono = txtTelefono.Text.Trim().ToUpper();
                        cli.Email = txtEmail.Text.Trim();
                    }
                    db.SaveChanges();
                    MessageBox.Show("Cliente guardado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            txtConsultaCliente.Text = "";
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            txtNombre.Focus();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente para eliminar.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    // Verificar si tiene registros de estiba asociados
                    var tieneRegistros = db.Registros.Any(r => r.IdCliente == _idSeleccionado.Value);
                    if (tieneRegistros)
                    {
                        MessageBox.Show("No se puede eliminar el cliente porque tiene registros de estiba asociados.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Preguntar si está seguro de eliminar
                    var respuesta = MessageBox.Show(
                        "¿Está seguro que desea eliminar el cliente seleccionado?",
                        "Confirmación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (respuesta != DialogResult.Yes) return;

                    var cli = db.Clientes.Find(_idSeleccionado.Value);
                    if (cli != null)
                    {
                        db.Clientes.Remove(cli);
                        db.SaveChanges();
                        MessageBox.Show("Cliente eliminado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void dgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvClientes.CurrentRow == null || dgvClientes.CurrentRow.Index < 0)
                return;
            if (!dgvClientes.CurrentRow.Selected)
                return;

            var row = dgvClientes.CurrentRow;
            if (row.Cells["IdCliente"] == null) return;

            _idSeleccionado = (int?)row.Cells["IdCliente"].Value;
            if (_idSeleccionado == null) return;

            // Cargar campos en TextBox
            txtNombre.Text = row.Cells["Nombre"].Value?.ToString().ToUpper() ?? "";
            txtRUC.Text = row.Cells["RUC"].Value?.ToString().ToUpper() ?? "";
            txtDireccion.Text = row.Cells["Direccion"].Value?.ToString().ToUpper() ?? "";
            txtTelefono.Text = row.Cells["Telefono"].Value?.ToString().ToUpper() ?? "";
            txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string nombreBuscado = txtConsultaCliente.Text.Trim();

            if (string.IsNullOrEmpty(nombreBuscado))
            {
                MessageBox.Show("Ingrese un nombre o parte del nombre para consultar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimpiarYEnfocar(txtConsultaCliente);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    // Busca clientes cuyo nombre contenga la cadena ingresada (sin importar mayúsculas/minúsculas)
                    var resultados = db.Clientes
                        .Where(c => c.Nombre.ToUpper().Contains(nombreBuscado.ToUpper()))
                        .OrderBy(c => c.Nombre)
                        .Select(c => new
                        {
                            c.IdCliente,
                            c.Nombre,
                            c.RUC,
                            c.Direccion,
                            c.Telefono,
                            c.Email
                        })
                        .ToList();

                    if (resultados.Count == 0)
                    {
                        MessageBox.Show("No se encontraron clientes que coincidan con la búsqueda.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dgvClientes.DataSource = null;
                        return;
                    }

                    // Mostrar resultados en DataGridView
                    dgvClientes.DataSource = resultados;
                    dgvClientes.ClearSelection();
                    _idSeleccionado = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar clientes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnListar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }
    }
}
