using Microsoft.Win32;
using ProyectoPICGestiónRecepción.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Drawing;



namespace ProyectoPICGestiónRecepción.Forms
{
    public partial class FormRegistroEstiba : Form
    {
        private int? _idSeleccionado = null; // ID del registro seleccionado

        public FormRegistroEstiba()
        {
            InitializeComponent();

            // Evento para cargar los campos al seleccionar fila en DataGridView
            dgvRegistros.SelectionChanged += DgvRegistros_SelectionChanged;

            // Configuración de DateTimePickers para solo hora
            dtpHoraInicio.Format = DateTimePickerFormat.Time;
            dtpHoraInicio.ShowUpDown = true;

            dtpHoraFinal.Format = DateTimePickerFormat.Time;
            dtpHoraFinal.ShowUpDown = true;
        }

        // Metodo de carga inicial
        private void FormRegistroEstiba_Load(object sender, EventArgs e)
        {
            CargarCombos();
            CargarDatos();
            LimpiarFormulario();
        }

        // Metodo de carga de combos
        private void CargarCombos()
        {
            using (var db = new AppDbContext())
            {
                // Clientes: mostrar Nombre completo
                cmbCliente.DataSource = db.Clientes
                    .Select(c => new { c.IdCliente, NombreCompleto = c.Nombre})
                    .ToList();
                cmbCliente.DisplayMember = "NombreCompleto";
                cmbCliente.ValueMember = "IdCliente";
                cmbCliente.SelectedIndex = -1;

                // Proveedores
                cmbProveedor.DataSource = db.Proveedores
                    .Select(p => new { p.IdProveedor, NombreCompleto = p.Nombre })
                    .ToList();
                cmbProveedor.DisplayMember = "NombreCompleto";
                cmbProveedor.ValueMember = "IdProveedor";
                cmbProveedor.SelectedIndex = -1;

                // Responsables
                cmbResponsable.DataSource = db.Responsables
                    .Select(r => new { r.IdResponsable, NombreCompleto = r.Nombre + " " + r.Apellido })
                    .ToList();
                cmbResponsable.DisplayMember = "NombreCompleto";
                cmbResponsable.ValueMember = "IdResponsable";
                cmbResponsable.SelectedIndex = -1;

                // Vehículos
                var vehiculos = db.Vehiculos
                    .Select(v => new { v.IdVehiculo, v.Placa, v.TipoVehiculo })
                    .ToList();
                cmbPlaca.DataSource = vehiculos;
                cmbPlaca.DisplayMember = "Placa";
                cmbPlaca.ValueMember = "IdVehiculo";
                cmbPlaca.SelectedIndex = -1;
                cmbPlaca.DropDownStyle = ComboBoxStyle.DropDown; // editable

                cmbTipoVehiculo.DataSource = new string[]
                {
                    "Tarifa especial", "Descarga especial", "Liviano", "Sencillo", "Mula", "Contenedor"
                };
                cmbTipoVehiculo.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }

        // Metodo para limpiar formulario
        private void LimpiarFormulario()
        {
            dtpFecha.Value = DateTime.Now;
            dtpHoraInicio.Value = DateTime.Now;
            dtpHoraFinal.Value = DateTime.Now;
            cmbCliente.SelectedIndex = -1;
            cmbProveedor.SelectedIndex = -1;
            cmbResponsable.SelectedIndex = -1;
            cmbPlaca.Text = "";
            cmbTipoVehiculo.SelectedIndex = -1;
            txtObservaciones.Text = "";
            _idSeleccionado = null;
            errorProvider1.Clear();
        }

        // Metodo de validación de formulario
        private bool ValidarFormulario()
        {
            bool ok = true;
            errorProvider1.Clear();

            // Validaciones obligatorias
            if (cmbCliente.SelectedIndex < 0)
            {
                errorProvider1.SetError(cmbCliente, "Seleccione un cliente");
                ok = false;
            }
            if (cmbProveedor.SelectedIndex < 0)
            {
                errorProvider1.SetError(cmbProveedor, "Seleccione un proveedor");
                ok = false;
            }
            if (cmbResponsable.SelectedIndex < 0)
            {
                errorProvider1.SetError(cmbResponsable, "Seleccione un responsable");
                ok = false;
            }
            if (string.IsNullOrWhiteSpace(cmbPlaca.Text))
            {
                errorProvider1.SetError(cmbPlaca, "Ingrese placa del vehículo");
                ok = false;
            }
            if (cmbTipoVehiculo.SelectedIndex < 0)
            {
                errorProvider1.SetError(cmbTipoVehiculo, "Seleccione tipo de vehículo");
                ok = false;
            }
            if (dtpHoraInicio.Value.TimeOfDay >= dtpHoraFinal.Value.TimeOfDay)
            {
                errorProvider1.SetError(dtpHoraFinal, "Hora final debe ser mayor que hora inicio");
                ok = false;
            }

            return ok;
        }

        // Metodo para obtener o crear vehículo si no existe
        private int ObtenerIdVehiculo(string placa, string tipo)
        {
            using (var db = new AppDbContext())
            {
                var veh = db.Vehiculos.FirstOrDefault(v => v.Placa == placa);
                if (veh != null) return veh.IdVehiculo;

                // Crear nuevo vehículo
                var nuevoVeh = new Vehiculo { Placa = placa, TipoVehiculo = tipo };
                db.Vehiculos.Add(nuevoVeh);
                db.SaveChanges();
                CargarCombos(); // actualizar combos
                return nuevoVeh.IdVehiculo;
            }
        }

        // Metodo del botón Guardar
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                using (var db = new AppDbContext())
                {
                    int idVehiculo = ObtenerIdVehiculo(cmbPlaca.Text.Trim(), cmbTipoVehiculo.Text.Trim());

                    if (_idSeleccionado == null)
                    {
                        // Crear nuevo registro
                        var reg = new RegistroEstiba
                        {
                            Fecha = dtpFecha.Value.Date,
                            HoraInicio = dtpHoraInicio.Value.TimeOfDay,
                            HoraFinal = dtpHoraFinal.Value.TimeOfDay,
                            IdCliente = (int)cmbCliente.SelectedValue,
                            IdProveedor = (int)cmbProveedor.SelectedValue,
                            IdResponsable = (int)cmbResponsable.SelectedValue,
                            IdVehiculo = idVehiculo,
                            Observaciones = txtObservaciones.Text.Trim()
                        };
                        db.Registros.Add(reg);
                    }
                    else
                    {
                        // Editar existente
                        var reg = db.Registros.Find(_idSeleccionado.Value);
                        if (reg == null) return;

                        reg.Fecha = dtpFecha.Value.Date;
                        reg.HoraInicio = dtpHoraInicio.Value.TimeOfDay;
                        reg.HoraFinal = dtpHoraFinal.Value.TimeOfDay;
                        reg.IdCliente = (int)cmbCliente.SelectedValue;
                        reg.IdProveedor = (int)cmbProveedor.SelectedValue;
                        reg.IdResponsable = (int)cmbResponsable.SelectedValue;
                        reg.IdVehiculo = idVehiculo;
                        reg.Observaciones = txtObservaciones.Text.Trim();

                        db.SaveChanges();
                        MessageBox.Show("✏️ Registro actualizado correctamente");
                    }

                    db.SaveChanges();
                    MessageBox.Show("Registro guardado correctamente");
                }

                CargarDatos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar registro: " + ex.Message);
            }
        }

        // Metodo para cargar datos en DataGridView
        private void CargarDatos()
        {
            using (var db = new AppDbContext())
            {
                            var lista = db.Registros
                .Include(r => r.Cliente)
                .Include(r => r.Proveedor)
                .Include(r => r.ResponsableEstiba)
                .Include(r => r.Vehiculo)
                .OrderByDescending(r => r.Fecha)
                .AsEnumerable() //  fuerza que de aquí en adelante sea LINQ to Objects
                .Select(r => new
                {
                    r.IdRegistro,
                    Fecha = r.Fecha,
                    HoraInicio = r.HoraInicio.ToString(@"hh\:mm"), // ya en memoria
                    HoraFinal = r.HoraFinal.ToString(@"hh\:mm"),
                    Cliente = r.Cliente.Nombre,   
                    Proveedor = r.Proveedor.Nombre,
                    Responsable = r.ResponsableEstiba.Nombre + " " + r.ResponsableEstiba.Apellido,
                    VehiculoPlaca = r.Vehiculo.Placa,
                    TipoVehiculo = r.Vehiculo.TipoVehiculo,
                    r.Observaciones
                })
                .ToList();


                dgvRegistros.DataSource = lista;
                dgvRegistros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvRegistros.ClearSelection();
                _idSeleccionado = null;
            }
        }

        // Metodo del botón Nuevo
        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // Metodo del botón Editar
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvRegistros.CurrentRow == null || dgvRegistros.CurrentRow.Index < 0) return;

            _idSeleccionado = (int)dgvRegistros.CurrentRow.Cells["IdRegistro"].Value;

            using (var db = new AppDbContext())
            {
                var reg = db.Registros
                    .Include(r => r.Cliente)
                    .Include(r => r.Proveedor)
                    .Include(r => r.ResponsableEstiba)
                    .Include(r => r.Vehiculo)
                    .FirstOrDefault(r => r.IdRegistro == _idSeleccionado);

                if (reg != null)
                {
                    dtpFecha.Value = reg.Fecha;
                    dtpHoraInicio.Value = DateTime.Today.Add(reg.HoraInicio);
                    dtpHoraFinal.Value = DateTime.Today.Add(reg.HoraFinal);

                    cmbCliente.SelectedValue = reg.IdCliente;
                    cmbProveedor.SelectedValue = reg.IdProveedor;
                    cmbResponsable.SelectedValue = reg.IdResponsable;

                    cmbPlaca.Text = reg.Vehiculo.Placa;
                    cmbTipoVehiculo.Text = reg.Vehiculo.TipoVehiculo;
                    txtObservaciones.Text = reg.Observaciones;
                }
            }
        }


        // Metodo del botón Eliminar
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_idSeleccionado == null)
            {
                MessageBox.Show("Seleccione un registro para eliminar");
                return;
            }

            using (var db = new AppDbContext())
            {
                var respuesta = MessageBox.Show(
                    "¿Está seguro que desea eliminar el registro seleccionado?",
                    "Confirmación", MessageBoxButtons.YesNo);
                    
                    var reg = db.Registros.Find(_idSeleccionado.Value);
                if (reg != null)
                {
                    db.Registros.Remove(reg);
                    db.SaveChanges();
                    MessageBox.Show("Registro eliminado correctamente");
                }
            }

            CargarDatos();
            LimpiarFormulario();
        }

        // Metodo del botón Consultar por fecha
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            ConsultarRegistrosPorFecha();
        }

        // Metodo para consultar registros solo por fecha
        private void ConsultarRegistrosPorFecha()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    DateTime fecha = dtpFechaConsulta.Value.Date;

                    var registros = db.Registros
                        .Include(r => r.Cliente)
                        .Include(r => r.Proveedor)
                        .Include(r => r.ResponsableEstiba)
                        .Include(r => r.Vehiculo)
                        .Where(r => r.Fecha == fecha)
                        .OrderBy(r => r.HoraInicio)
                        .AsEnumerable() // fuerza pasar a memoria
                        .Select(r => new
                        {
                            r.IdRegistro,
                            r.Fecha,
                            HoraInicio = r.HoraInicio.ToString(@"hh\:mm"), // ya en memoria
                            HoraFinal = r.HoraFinal.ToString(@"hh\:mm"),
                            Cliente = r.Cliente.Nombre,   
                            Proveedor = r.Proveedor.Nombre, 
                            Responsable = r.ResponsableEstiba.Nombre + " " + r.ResponsableEstiba.Apellido,
                            VehiculoPlaca = r.Vehiculo.Placa,
                            TipoVehiculo = r.Vehiculo.TipoVehiculo,
                            r.Observaciones
                        })
                        .ToList();

                    dgvRegistros.DataSource = registros;
                    dgvRegistros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dgvRegistros.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar registros por fecha: {ex.Message}");
            }
        }


        // Metodo del botón Listar todos
        private void btnListar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }

        // Metodo que carga el registro seleccionado en el formulario
        private void DgvRegistros_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRegistros.CurrentRow == null || dgvRegistros.CurrentRow.Index < 0)
            {
                _idSeleccionado = null;
                return;
            }

            _idSeleccionado = (int?)dgvRegistros.CurrentRow.Cells["IdRegistro"].Value;
            CargarRegistroSeleccionado();
        }

        // Metodo auxiliar para llenar formulario con el registro seleccionado
        private void CargarRegistroSeleccionado()
        {
            if (_idSeleccionado == null) return;

            using (var db = new AppDbContext())
            {
                var registro = db.Registros
                    .Include(r => r.Cliente)
                    .Include(r => r.Proveedor)
                    .Include(r => r.ResponsableEstiba)
                    .Include(r => r.Vehiculo)
                    .FirstOrDefault(r => r.IdRegistro == _idSeleccionado.Value);

                if (registro != null)
                {
                    dtpFecha.Value = registro.Fecha;
                    dtpHoraInicio.Value = DateTime.Today.Add(registro.HoraInicio);
                    dtpHoraFinal.Value = DateTime.Today.Add(registro.HoraFinal);

                    cmbCliente.SelectedValue = registro.IdCliente;
                    cmbProveedor.SelectedValue = registro.IdProveedor;
                    cmbResponsable.SelectedValue = registro.IdResponsable;

                    cmbPlaca.Text = registro.Vehiculo.Placa;
                    cmbTipoVehiculo.SelectedItem = registro.Vehiculo.TipoVehiculo;

                    txtObservaciones.Text = registro.Observaciones;
                }
            }
        }
        private decimal ObtenerCostoPorTipo(string tipoVehiculo)
        {
            switch (tipoVehiculo)
            {
                case "Tarifa Especial": return 15;
                case "Descarga Especial": return 20;
                case "Liviano": return 35;
                case "Sencillo": return 45;
                case "Mula": return 60;
                case "Contenedor": return 80;
                default: return 0;
            }
        }


        // Exportar a Excel con facturación
        private void ExportarExcel(List<RegistroEstiba> registros)
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Registros");

                // ====== ENCABEZADO EMPRESA ======
                ws.Range("A1:H1").Merge();
                ws.Cell(1, 1).Value = "GROUP SERVICE - REPORTE DE REGISTROS";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
                ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.DarkBlue;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(1).Height = 30; // más alto para el logo

                // ====== OPCIONAL: INSERTAR LOGO ======
                string logoPath = @"C:\Ruta\logo.png";
                if (System.IO.File.Exists(logoPath))
                {
                    var image = ws.AddPicture(logoPath)
                        .MoveTo(ws.Cell(1, 1), 5, 5) // margen desde A1
                        .Scale(0.3); // tamaño del logo
                }

                // Dejar una fila vacía antes de la tabla
                int filaEncabezado = 3;

                // ====== ENCABEZADOS DE TABLA ======
                string[] encabezados = { "Fecha", "Cliente", "Proveedor", "Vehículo", "Tipo Vehículo", "Responsable", "Observaciones", "Costo ($)" };
                for (int i = 0; i < encabezados.Length; i++)
                {
                    var cell = ws.Cell(filaEncabezado, i + 1);
                    cell.Value = encabezados[i];

                    // Estilo encabezados
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                int fila = filaEncabezado + 1;
                decimal total = 0;

                foreach (var r in registros)
                {
                    decimal costo = ObtenerCostoPorTipo(r.Vehiculo.TipoVehiculo);
                    total += costo;

                    ws.Cell(fila, 1).Value = r.Fecha.ToShortDateString();
                    ws.Cell(fila, 2).Value = r.Cliente.Nombre;
                    ws.Cell(fila, 3).Value = r.Proveedor.Nombre;
                    ws.Cell(fila, 4).Value = r.Vehiculo.Placa;
                    ws.Cell(fila, 5).Value = r.Vehiculo.TipoVehiculo;
                    ws.Cell(fila, 6).Value = r.ResponsableEstiba.Nombre + " " + r.ResponsableEstiba.Apellido;
                    ws.Cell(fila, 7).Value = r.Observaciones;
                    ws.Cell(fila, 8).Value = costo;

                    // Estilo filas
                    for (int col = 1; col <= 8; col++)
                    {
                        var cell = ws.Cell(fila, col);
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        // Filas alternadas (estilo zebra)
                        if (fila % 2 == 0)
                            cell.Style.Fill.BackgroundColor = XLColor.White;
                        else
                            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    fila++;
                }

                // ====== TOTAL ======
                ws.Cell(fila, 7).Value = "TOTAL:";
                ws.Cell(fila, 7).Style.Font.Bold = true;
                ws.Cell(fila, 8).Value = total;
                ws.Cell(fila, 8).Style.Font.Bold = true;
                ws.Cell(fila, 8).Style.Fill.BackgroundColor = XLColor.LightGreen;

                // ====== FORMATOS ======
                ws.Column(8).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Columns().AdjustToContents();

                // Guardar archivo
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Workbook|*.xlsx",
                    Title = "Guardar reporte de registros",
                    FileName = "Reporte_GroupService.xlsx" // Nombre por defecto
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    wb.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Reporte exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }



        private void btnExportar_Click(object sender, EventArgs e)
        {
            using (var db = new AppDbContext())
            {
                // Traer los registros (puedes filtrar si quieres, ahora todos)
                var registros = db.Registros
                    .Include(r => r.Cliente)
                    .Include(r => r.Proveedor)
                    .Include(r => r.ResponsableEstiba)
                    .Include(r => r.Vehiculo)
                    .ToList();

                if (registros.Count == 0)
                {
                    MessageBox.Show("No hay registros para exportar.");
                    return;
                }

                ExportarExcel(registros); // Llamar al método
            }
        }
    }
}
