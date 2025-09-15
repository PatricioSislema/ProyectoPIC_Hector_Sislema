using ProyectoPICGestiónRecepción.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

namespace ProyectoPICGestiónRecepción.Forms
{
    public partial class FormProveedor : Form
    {
        private int? _idSeleccionado = null;

        public FormProveedor()
        {
            InitializeComponent();
        }

        private void FormProveedor_Load(object sender, EventArgs e)
        {
            CargarRepresentantes();
            CargarDatos();
            LimpiarFormulario();
            ConfigurarAutoCompleteNombre();
        }

        
        // Cargar representantes en ComboBox
        
        private void CargarRepresentantes()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var reps = db.Representantes
                        .OrderBy(r => r.Nombre)
                        .Select(r => new
                        {
                            r.IdRepresentante,
                            NombreCompleto = r.Nombre + " " + r.Apellido
                        })
                        .ToList();

                    cmbRepresentante.DataSource = reps;
                    cmbRepresentante.DisplayMember = "NombreCompleto";
                    cmbRepresentante.ValueMember = "IdRepresentante";
                    cmbRepresentante.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar representantes: {ex.Message}");
            }
        }

        // Autocompletar para consulta
       
        private void ConfigurarAutoCompleteNombre()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var nombres = db.Proveedores.Select(p => p.Nombre.ToUpper()).ToArray();
                    var autoComplete = new AutoCompleteStringCollection();
                    autoComplete.AddRange(nombres);

                    txtConsultaProveedor.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    txtConsultaProveedor.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    txtConsultaProveedor.AutoCompleteCustomSource = autoComplete;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al configurar autocompletado: {ex.Message}");
            }
        }

        // Cargar datos al DataGridView
      
        private void CargarDatos()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var proveedores = db.Proveedores
                        .Include(p => p.Representante)
                        .OrderBy(p => p.Nombre)
                        .Select(p => new
                        {
                            p.IdProveedor,
                            p.Nombre,
                            p.IdRepresentante,
                            NombreRepresentante = p.Representante != null
                                ? p.Representante.Nombre + " " + p.Representante.Apellido
                                : ""
                        })
                        .ToList();

                    dgvProveedores.DataSource = proveedores;
                    if (dgvProveedores.Columns["IdProveedor"] != null)
                        dgvProveedores.Columns["IdProveedor"].HeaderText = "Código";

                    dgvProveedores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dgvProveedores.ClearSelection();
                    _idSeleccionado = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}");
            }
        }

        
        // Limpiar formulario
        
        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            cmbRepresentante.SelectedIndex = -1;
            txtConsultaProveedor.Text = "";
            errorProvider1.Clear();
            _idSeleccionado = null;
        }

        private void LimpiarYEnfocar(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.Clear();
            textBox.Focus();
        }

        
        // Validar formulario
     
        private bool ValidarFormulario()
        {
            bool ok = true;
            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errorProvider1.SetError(txtNombre, "Nombre obligatorio");
                LimpiarYEnfocar(txtNombre);
                ok = false;
            }

            if (cmbRepresentante.SelectedIndex < 0)
            {
                errorProvider1.SetError(cmbRepresentante, "Debe seleccionar un representante");
                cmbRepresentante.Focus();
                ok = false;
            }

            return ok;
        }

        
        // Botones
      
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
                        var prov = new Proveedor
                        {
                            Nombre = txtNombre.Text.Trim().ToUpper(),
                            IdRepresentante = (int)cmbRepresentante.SelectedValue
                        };
                        db.Proveedores.Add(prov);
                    }
                    else
                    {
                        var prov = db.Proveedores.Find(_idSeleccionado.Value);
                        if (prov == null) return;
                        prov.Nombre = txtNombre.Text.Trim().ToUpper();
                        prov.IdRepresentante = (int)cmbRepresentante.SelectedValue;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Proveedor guardado correctamente", "Éxito");
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar proveedor: {ex.Message}");
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            txtConsultaProveedor.Text = "";
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un proveedor de la lista.", "Info");
                return;
            }
            txtNombre.Focus();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un proveedor para eliminar.", "Info");
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var respuesta = MessageBox.Show(
                        "¿Está seguro que desea eliminar el proveedor seleccionado?",
                        "Confirmación", MessageBoxButtons.YesNo
                    );

                    if (respuesta != DialogResult.Yes) return;

                    var prov = db.Proveedores.Find(_idSeleccionado.Value);
                    if (prov != null)
                    {
                        db.Proveedores.Remove(prov);
                        db.SaveChanges();
                        MessageBox.Show("Proveedor eliminado correctamente", "Éxito");
                    }
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar proveedor: {ex.Message}");
            }
        }

        // =======================
        // Selección del DataGrid
        // =======================
        private void dgvProveedores_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProveedores.CurrentRow == null) return;
            if (dgvProveedores.CurrentRow.Index < 0) return;

            var row = dgvProveedores.CurrentRow;

            _idSeleccionado = (int?)row.Cells["IdProveedor"].Value;

            txtNombre.Text = row.Cells["Nombre"].Value?.ToString().ToUpper() ?? "";

            if (row.Cells["IdRepresentante"]?.Value != null)
                cmbRepresentante.SelectedValue = (int)row.Cells["IdRepresentante"].Value;
            else
                cmbRepresentante.SelectedIndex = -1;
        }

        // =======================
        // Consultar proveedores
        // =======================
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string nombreBuscado = txtConsultaProveedor.Text.Trim();
            if (string.IsNullOrEmpty(nombreBuscado))
            {
                MessageBox.Show("Ingrese un nombre o parte del nombre para consultar.", "Aviso");
                LimpiarYEnfocar(txtConsultaProveedor);
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var resultados = db.Proveedores
                        .Include(p => p.Representante)
                        .Where(p => p.Nombre.ToUpper().Contains(nombreBuscado.ToUpper()))
                        .OrderBy(p => p.Nombre)
                        .Select(p => new
                        {
                            p.IdProveedor,
                            p.Nombre,
                            p.IdRepresentante,
                            NombreRepresentante = p.Representante != null
                                ? p.Representante.Nombre + " " + p.Representante.Apellido
                                : ""
                        })
                        .ToList();

                    if (resultados.Count == 0)
                    {
                        MessageBox.Show("No se encontraron proveedores.", "Info");
                        dgvProveedores.DataSource = null;
                        return;
                    }

                    dgvProveedores.DataSource = resultados;
                    dgvProveedores.ClearSelection();
                    _idSeleccionado = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar proveedores: {ex.Message}");
            }
        }

        private void btnListar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }
    }
}





