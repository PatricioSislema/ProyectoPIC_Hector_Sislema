using ProyectoPICGestiónRecepción.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

namespace ProyectoPICGestiónRecepción.Forms
{
    public partial class FormVehiculo : Form
    {
        private int? _idSeleccionado = null; // ID del registro seleccionado para editar/eliminar

        public FormVehiculo()
        {
            InitializeComponent();
            ConfigurarComboTipoVehiculo(); // Cargar tipos en el ComboBox
        }

        // Metodo de carga inicial
        private void FormVehiculo_Load(object sender, EventArgs e)
        {
            CargarDatos();
            ConfigurarAutoCompletePlaca();
            LimpiarFormulario();
        }

        // Metodo de configurar ComboBox de tipo de vehículo
        private void ConfigurarComboTipoVehiculo()
        {
            cmbTipoVehiculo.Items.Clear();
            cmbTipoVehiculo.Items.AddRange(new string[]
            {
                "Tarifa especial",
                "Descarga especial",
                "Liviano",
                "Sencillo",
                "Mula",
                "Contenedor"
            });
            cmbTipoVehiculo.SelectedIndex = -1;
        }

        // Metodo de autocompletado por placa
        private void ConfigurarAutoCompletePlaca()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var placas = db.Vehiculos.Select(v => v.Placa.ToUpper()).ToArray();
                    var autoComplete = new AutoCompleteStringCollection();
                    autoComplete.AddRange(placas);

                    txtConsultarVehiculo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtConsultarVehiculo.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtConsultarVehiculo.AutoCompleteCustomSource = autoComplete;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al configurar autocompletado: {ex.Message}");
            }
        }

        // Metodo de cargar datos al DataGridView
        private void CargarDatos()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var lista = db.Vehiculos
                                  .OrderBy(v => v.Placa)
                                  .Select(v => new
                                  {
                                      v.IdVehiculo,
                                      v.Placa,
                                      v.TipoVehiculo
                                  })
                                  .ToList();

                    dgvVehiculos.AutoGenerateColumns = true;
                    dgvVehiculos.DataSource = null;
                    dgvVehiculos.DataSource = lista;
                }

                dgvVehiculos.ClearSelection();
                _idSeleccionado = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar vehículos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo de limpiar formulario
        private void LimpiarFormulario()
        {
            txtPlaca.Text = "";
            cmbTipoVehiculo.SelectedIndex = -1;
            txtConsultarVehiculo.Text = "";
            _idSeleccionado = null;
            errorProvider1.Clear();
        }

        // Metodo de limpiar y enfocar un TextBox
        private void LimpiarYEnfocar(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.Clear();
            textBox.Focus();
        }

        // Metodo de validar formulario
        private bool ValidarFormulario()
        {
            bool ok = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtPlaca.Text))
            {
                errorProvider1.SetError(txtPlaca, "Placa obligatoria");
                LimpiarYEnfocar(txtPlaca);
                ok = false;
                return ok;
            }

            if (cmbTipoVehiculo.SelectedIndex < 0)
            {
                errorProvider1.SetError(cmbTipoVehiculo, "Debe seleccionar un tipo de vehículo");
                cmbTipoVehiculo.Focus();
                ok = false;
                return ok;
            }

            return ok;
        }


        // Metodo de botón Nuevo
        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtPlaca.Focus();
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
                        // Nuevo vehículo
                        var veh = new Vehiculo
                        {
                            Placa = txtPlaca.Text.Trim().ToUpper(),
                            TipoVehiculo = cmbTipoVehiculo.SelectedItem.ToString()
                        };
                        db.Vehiculos.Add(veh);
                    }
                    else
                    {
                        // Editar vehículo existente
                        var veh = db.Vehiculos.Find(_idSeleccionado.Value);
                        if (veh == null) return;
                        veh.Placa = txtPlaca.Text.Trim().ToUpper();
                        veh.TipoVehiculo = cmbTipoVehiculo.SelectedItem.ToString();
                    }

                    db.SaveChanges();
                    MessageBox.Show("Vehículo guardado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarDatos();
                LimpiarFormulario();
                ConfigurarAutoCompletePlaca(); // Refrescar autocompletado
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar vehículo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo de botón Editar
        private void btnEditar_Click(object sender, EventArgs e)
        {
            txtConsultarVehiculo.Text = "";
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un vehículo de la lista.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            txtPlaca.Focus();
        }

        // Metodo de botón Eliminar
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un vehículo para eliminar.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var respuesta = MessageBox.Show(
                        "¿Está seguro que desea eliminar el vehículo seleccionado?",
                        "Confirmación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (respuesta != DialogResult.Yes) return;

                    var veh = db.Vehiculos.Find(_idSeleccionado.Value);
                    if (veh != null)
                    {
                        db.Vehiculos.Remove(veh);
                        db.SaveChanges();
                        MessageBox.Show("Vehículo eliminado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                CargarDatos();
                LimpiarFormulario();
                ConfigurarAutoCompletePlaca(); // Refrescar autocompletado
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar vehículo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo de botón Consultar
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string placaBuscada = txtConsultarVehiculo.Text.Trim();

            if (string.IsNullOrEmpty(placaBuscada))
            {
                MessageBox.Show("Ingrese una placa o parte de ella para consultar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimpiarYEnfocar(txtConsultarVehiculo);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var resultados = db.Vehiculos
                        .Where(v => v.Placa.ToUpper().Contains(placaBuscada.ToUpper()))
                        .OrderBy(v => v.Placa)
                        .Select(v => new
                        {
                            v.IdVehiculo,
                            v.Placa,
                            v.TipoVehiculo
                        })
                        .ToList();

                    if (resultados.Count == 0)
                    {
                        MessageBox.Show("No se encontraron vehículos que coincidan con la búsqueda.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dgvVehiculos.DataSource = null;
                        return;
                    }

                    dgvVehiculos.AutoGenerateColumns = true;
                    dgvVehiculos.DataSource = null;
                    dgvVehiculos.DataSource = resultados;
                    dgvVehiculos.ClearSelection();
                    _idSeleccionado = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar vehículos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Metodo de selección en DataGridView
        private void dgvVehiculos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvVehiculos.CurrentRow == null || dgvVehiculos.CurrentRow.Index < 0) return;
            if (!dgvVehiculos.CurrentRow.Selected) return;

            var row = dgvVehiculos.CurrentRow;
            if (row.Cells["IdVehiculo"] == null) return;

            _idSeleccionado = (int?)row.Cells["IdVehiculo"].Value;
            if (_idSeleccionado == null) return;

            txtPlaca.Text = row.Cells["Placa"].Value?.ToString().ToUpper() ?? "";
            cmbTipoVehiculo.SelectedItem = row.Cells["TipoVehiculo"].Value?.ToString() ?? null;
        }

        private void btnListar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }
    }
}


