using ProyectoPICGestiónRecepción.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

namespace ProyectoPICGestiónRecepción.Forms
{
    public partial class FormRepresentante : Form
    {
        private int? _idSeleccionado = null; // Metodo de seguimiento de registro seleccionado

        public FormRepresentante()
        {
            InitializeComponent();
        }

        // Metodo de carga inicial
        private void FormRepresentante_Load(object sender, EventArgs e)
        {
            CargarDatos();
            LimpiarFormulario();
            ConfigurarAutoCompleteNombre();
        }

        // Metodo de cargar datos al DataGridView
        private void CargarDatos()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var lista = db.Representantes
                        .OrderBy(r => r.Nombre)
                        .Select(r => new
                        {
                            r.IdRepresentante,
                            r.Nombre,
                            r.Apellido,
                            r.Cedula
                        })
                        .ToList();

                    dgvRepresentantes.DataSource = lista;
                    dgvRepresentantes.ClearSelection();
                    _idSeleccionado = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar representantes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarAutoCompleteNombre()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var nombres = db.Representantes.Select(p => p.Nombre.ToUpper()).ToArray();
                    var autoComplete = new AutoCompleteStringCollection();
                    autoComplete.AddRange(nombres);

                    txtConsultarRepresentante.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtConsultarRepresentante.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtConsultarRepresentante.AutoCompleteCustomSource = autoComplete;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al configurar autocompletado: {ex.Message}");
            }
        }
        // Metodo de limpiar formulario
        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtCedula.Text = "";
            txtConsultarRepresentante.Text = "";
            errorProvider1.Clear();
            _idSeleccionado = null;
        }

        // Metodo de validar formulario
        private bool ValidarFormulario()
        {
            bool ok = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errorProvider1.SetError(txtNombre, "Nombre obligatorio");
                txtNombre.Focus();
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                errorProvider1.SetError(txtApellido, "Apellido obligatorio");
                txtApellido.Focus();
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                errorProvider1.SetError(txtCedula, "Cédula obligatoria");
                txtCedula.Focus();
                ok = false;
            }
            else if (txtCedula.Text.Length != 10)
            {
                errorProvider1.SetError(txtCedula, "Cédula debe tener 10 caracteres");
                LimpiarYEnfocar(txtCedula);
                ok = false;
            }

            return ok;
        }

        // Metodo de botón Nuevo
        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombre.Focus();
        }
        private void LimpiarYEnfocar(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.Clear();
            textBox.Focus();
        }

        // Metodo de botón Guardar
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    if (_idSeleccionado == null)
                    {
                        // Nuevo representante
                        var rep = new Representante
                        {
                            Nombre = txtNombre.Text.Trim().ToUpper(),
                            Apellido = txtApellido.Text.Trim().ToUpper(),
                            Cedula = txtCedula.Text.Trim().ToUpper()
                        };
                        db.Representantes.Add(rep);
                    }
                    else
                    {
                        // Editar representante
                        var rep = db.Representantes.Find(_idSeleccionado.Value);
                        if (rep != null)
                        {
                            rep.Nombre = txtNombre.Text.Trim().ToUpper();
                            rep.Apellido = txtApellido.Text.Trim().ToUpper();
                            rep.Cedula = txtCedula.Text.Trim().ToUpper();
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("Representante guardado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar representante: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo de botón Editar
        private void btnEditar_Click(object sender, EventArgs e)
        {
            txtConsultarRepresentante.Text = "";
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un representante de la lista.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            txtNombre.Focus();
        }

        // Metodo de botón Eliminar
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un representante para eliminar.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var respuesta = MessageBox.Show("¿Está seguro que desea eliminar el representante seleccionado?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (respuesta != DialogResult.Yes) return;

                    var rep = db.Representantes.Find(_idSeleccionado.Value);
                    if (rep != null)
                    {
                        db.Representantes.Remove(rep);
                        db.SaveChanges();
                        MessageBox.Show("Representante eliminado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar representante: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo de selección en DataGridView
        private void dgvRepresentantes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRepresentantes.CurrentRow == null || dgvRepresentantes.CurrentRow.Index < 0) return;
            if (!dgvRepresentantes.CurrentRow.Selected) return;

            var row = dgvRepresentantes.CurrentRow;
            _idSeleccionado = (int?)row.Cells["IdRepresentante"].Value;
            if (_idSeleccionado == null) return;

            txtNombre.Text = row.Cells["Nombre"].Value?.ToString();
            txtApellido.Text = row.Cells["Apellido"].Value?.ToString();
            txtCedula.Text = row.Cells["Cedula"].Value?.ToString();
        }

        // Metodo de botón Consultar
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string nombreBuscado = txtConsultarRepresentante.Text.Trim();

            try
            {
                using (var db = new AppDbContext())
                {
                    var resultados = db.Representantes
                        .Where(r => r.Nombre.ToUpper().Contains(nombreBuscado.ToUpper()) ||
                                    r.Apellido.ToUpper().Contains(nombreBuscado.ToUpper()))
                        .OrderBy(r => r.Nombre)
                        .Select(r => new
                        {
                            r.IdRepresentante,
                            r.Nombre,
                            r.Apellido,
                            r.Cedula
                        })
                        .ToList();

                    dgvRepresentantes.DataSource = resultados;
                    dgvRepresentantes.ClearSelection();
                    _idSeleccionado = null;

                    if (resultados.Count == 0)
                        MessageBox.Show("No se encontraron representantes que coincidan con la búsqueda.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al consultar representantes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnListar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }
    }
}


