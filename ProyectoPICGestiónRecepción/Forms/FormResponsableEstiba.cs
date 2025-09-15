using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProyectoPICGestiónRecepción.Data;


namespace ProyectoPICGestiónRecepción.Forms
{
    public partial class FormResponsableEstiba : Form
    {
        private int? _idSeleccionado = null; // ID del registro seleccionado

        public FormResponsableEstiba()
        {
            InitializeComponent();
        }

        // Metodo de carga inicial
        private void FormResponsableEstiba_Load(object sender, EventArgs e)
        {
            CargarDatos();
            LimpiarFormulario();
            ConfigurarAutoCompleteNombre();
        }

        // Metodo de autocompletado por nombre
        private void ConfigurarAutoCompleteNombre()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var nombres = db.Responsables.Select(r => r.Nombre.ToUpper()).ToArray();
                    var autoComplete = new AutoCompleteStringCollection();
                    autoComplete.AddRange(nombres);

                    txtConsultarResponsable.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtConsultarResponsable.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtConsultarResponsable.AutoCompleteCustomSource = autoComplete;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al configurar autocompletado: {ex.Message}");
            }
        }

        // Metodo de carga de datos al DataGridView
        private void CargarDatos()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var lista = db.Responsables
                                  .OrderBy(r => r.Nombre)
                                  .Select(r => new
                                  {
                                      r.IdResponsable,
                                      r.Nombre,
                                      r.Apellido,
                                      r.Cedula
                                  })
                                  .ToList();

                    dgvResponsables.DataSource = lista;
                    dgvResponsables.ClearSelection();
                    _idSeleccionado = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar responsables: {ex.Message}");
            }
        }

        // Metodo para limpiar formulario
        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtCedula.Text = "";
            txtConsultarResponsable.Text = "";
            _idSeleccionado = null;
            errorProvider1.Clear();
        }

        // Metodo de validación del formulario
        private bool ValidarFormulario()
        {
            bool ok = true;
            errorProvider1.Clear();

            // Validar nombre: obligatorio y solo letras
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || !Regex.IsMatch(txtNombre.Text, @"^[a-zA-Z\s]+$"))
            {
                errorProvider1.SetError(txtNombre, "Nombre obligatorio y solo letras");
                txtNombre.Focus();
                ok = false;
            }

            // Validar apellido: obligatorio y solo letras
            if (string.IsNullOrWhiteSpace(txtApellido.Text) || !Regex.IsMatch(txtApellido.Text, @"^[a-zA-Z\s]+$"))
            {
                errorProvider1.SetError(txtApellido, "Apellido obligatorio y solo letras");
                txtApellido.Focus();
                ok = false;
            }

            // Validar cédula: 10 dígitos
            if (string.IsNullOrWhiteSpace(txtCedula.Text) || !Regex.IsMatch(txtCedula.Text, @"^\d{10}$"))
            {
                errorProvider1.SetError(txtCedula, "Cédula debe tener 10 dígitos numéricos");
                txtCedula.Focus();
                ok = false;
            }

            return ok;
        }

        // Metodo del botón Nuevo
        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombre.Focus();
        }

        // Metodo del botón Guardar
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    if (_idSeleccionado == null)
                    {
                        // Guardar nuevo responsable
                        var resp = new ResponsableEstiba
                        {
                            Nombre = txtNombre.Text.Trim().ToUpper(),
                            Apellido = txtApellido.Text.Trim().ToUpper(),
                            Cedula = txtCedula.Text.Trim()
                        };
                        db.Responsables.Add(resp);
                    }
                    else
                    {
                        // Editar responsable existente
                        var resp = db.Responsables.Find(_idSeleccionado.Value);
                        if (resp == null) return;
                        resp.Nombre = txtNombre.Text.Trim().ToUpper();
                        resp.Apellido = txtApellido.Text.Trim().ToUpper();
                        resp.Cedula = txtCedula.Text.Trim();
                    }

                    db.SaveChanges();
                    MessageBox.Show("Responsable guardado correctamente");
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar responsable: {ex.Message}");
            }
        }

        // Metodo del botón Editar
        private void btnEditar_Click(object sender, EventArgs e)
        {
            txtConsultarResponsable.Text = "";
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un registro de la lista.");
                return;
            }
            txtNombre.Focus();
        }

        // Metodo del botón Eliminar
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un responsable para eliminar.");
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var resp = db.Responsables.Find(_idSeleccionado.Value);
                    if (resp != null)
                    {
                        db.Responsables.Remove(resp);
                        db.SaveChanges();
                        MessageBox.Show("Responsable eliminado correctamente");
                    }
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar responsable: {ex.Message}");
            }
        }

        // Metodo del botón Consultar por nombre
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string nombreBuscado = txtConsultarResponsable.Text.Trim();

            if (string.IsNullOrEmpty(nombreBuscado))
            {
                MessageBox.Show("Ingrese un nombre o parte del nombre para consultar.");
                txtConsultarResponsable.Focus();
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var resultados = db.Responsables
                        .Where(r => r.Nombre.ToUpper().Contains(nombreBuscado.ToUpper()))
                        .OrderBy(r => r.Nombre)
                        .Select(r => new
                        {
                            r.IdResponsable,
                            r.Nombre,
                            r.Apellido,
                            r.Cedula
                        })
                        .ToList();

                    if (resultados.Count == 0)
                    {
                        MessageBox.Show("No se encontraron responsables.");
                        dgvResponsables.DataSource = null;
                        return;
                    }

                    dgvResponsables.DataSource = resultados;
                    dgvResponsables.ClearSelection();
                    _idSeleccionado = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar responsables: {ex.Message}");
            }
        }

        // Metodo del botón Listar todo
        private void btnListar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }

        // Metodo de selección en DataGridView
        private void dgvResponsables_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResponsables.CurrentRow == null || dgvResponsables.CurrentRow.Index < 0) return;
            if (!dgvResponsables.CurrentRow.Selected) return;

            var row = dgvResponsables.CurrentRow;
            _idSeleccionado = (int?)row.Cells["IdResponsable"].Value;

            txtNombre.Text = row.Cells["Nombre"].Value?.ToString().ToUpper();
            txtApellido.Text = row.Cells["Apellido"].Value?.ToString().ToUpper();
            txtCedula.Text = row.Cells["Cedula"].Value?.ToString();
        }
    }
}

