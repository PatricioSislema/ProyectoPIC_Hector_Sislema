# ProyectoPIC_Hector_Sislema
## IMPORTANTE LEER ESTE ARCHIVO

## 📌 Descripción

Este proyecto corresponde al sistema **PIC Gestión de Recepción**, desarrollado en C# con Windows Forms y SQL Server.  
El sistema permite:

- Registrar operaciones de recepción y estiba.
- Consultar historial de registros.
- Generar reportes en Excel con formato profesional.

---

## 🛠️ Tecnologías utilizadas

- **C# (.NET Framework / WinForms)**
- **SQL Server**
- **ClosedXML** (para exportar a Excel)
- **GitHub** (control de versiones)

---

## 🗄️ Base de Datos: GroupServicesDB

La base de datos contiene las siguientes tablas:

1. `Vehiculo`
2. `Cliente`
3. `Proveedor`
4. `RepresentanteProveedor`
5. `ResponsableEstiba`
6. `RegistroEstiba` (tabla central)

> **Importante:** las tablas deben crearse en este orden para evitar errores de llaves foráneas.

---

## 📝 Script SQL (con datos de prueba)

```sql
-- Crear la base de datos
CREATE DATABASE GroupServicesDB;
GO
USE GroupServicesDB;
GO

-- Tabla Vehiculo
CREATE TABLE Vehiculo (
    idVehiculo INT IDENTITY(1,1) PRIMARY KEY,
    placa NVARCHAR(10) NOT NULL,
    tipoVehiculo NVARCHAR(50) NOT NULL
);

-- Tabla Cliente
CREATE TABLE Cliente (
    idCliente INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    RUC NVARCHAR(20) NOT NULL,
    direccion NVARCHAR(150),
    telefono NVARCHAR(20),
    email NVARCHAR(50)
);

-- Tabla Proveedor
CREATE TABLE Proveedor (
    idProveedor INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    representanteProveedor NVARCHAR(100)
);

-- Tabla RepresentanteProveedor
CREATE TABLE RepresentanteProveedor (
    idRepresentante INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL,
    apellido NVARCHAR(50) NOT NULL,
    cedula NVARCHAR(20) NOT NULL,
    firma VARBINARY(MAX),
    idProveedor INT FOREIGN KEY REFERENCES Proveedor(idProveedor)
);

-- Tabla ResponsableEstiba
CREATE TABLE ResponsableEstiba (
    idResponsable INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL,
    apellido NVARCHAR(50) NOT NULL,
    cedula NVARCHAR(20) NOT NULL
);

-- Tabla RegistroEstiba
CREATE TABLE RegistroEstiba (
    idRegistro INT IDENTITY(1,1) PRIMARY KEY,
    fecha DATE NOT NULL,
    horaInicio TIME NOT NULL,
    horaFinal TIME NOT NULL,
    firma VARBINARY(MAX),
    observaciones NVARCHAR(250),
    idCliente INT FOREIGN KEY REFERENCES Cliente(idCliente),
    idVehiculo INT FOREIGN KEY REFERENCES Vehiculo(idVehiculo),
    idProveedor INT FOREIGN KEY REFERENCES Proveedor(idProveedor),
    idResponsable INT FOREIGN KEY REFERENCES ResponsableEstiba(idResponsable)
);

-- Datos de prueba

-- Vehículos
INSERT INTO Vehiculo (placa, tipoVehiculo) VALUES
('ABC123', 'Camión'),
('DEF456', 'Furgoneta'),
('GHI789', 'Camión'),
('JKL012', 'Camioneta');

-- Clientes
INSERT INTO Cliente (nombre, RUC, direccion, telefono, email) VALUES
('Supermercado Quito', '1790012345001', 'Av. Amazonas 1234', '022345678', 'contacto@superquito.com'),
('Distribuidora Andes', '1790012345002', 'Av. 10 de Agosto 567', '022345679', 'info@andes.com');

-- Proveedores
INSERT INTO Proveedor (nombre, representanteProveedor) VALUES
('Proveedor Frutas Ecuador', 'Carlos Pérez'),
('Proveedor Lácteos Quito', 'María López');

-- Representantes de Proveedor
INSERT INTO RepresentanteProveedor (nombre, apellido, cedula, firma, idProveedor) VALUES
('Carlos', 'Pérez', '1701234567', NULL, 1),
('María', 'López', '1707654321', NULL, 2);

-- Responsables de Estiba
INSERT INTO ResponsableEstiba (nombre, apellido, cedula) VALUES
('Juan', 'García', '1701122334'),
('Ana', 'Martínez', '1702233445');

-- Registro de Estiba
INSERT INTO RegistroEstiba (fecha, horaInicio, horaFinal, firma, observaciones, idCliente, idVehiculo, idProveedor, idResponsable) VALUES
('2025-09-12', '08:00', '09:30', NULL, 'Descarga completa sin incidencias', 1, 1, 1, 1),
('2025-09-12', '10:00', '11:00', NULL, 'Carga parcial por retraso del proveedor', 2, 2, 2, 2),
('2025-09-13', '07:30', '08:45', NULL, 'Operación realizada con éxito', 1, 3, 1, 2),
('2025-09-13', '09:00', '10:15', NULL, 'Carga completada sin problemas', 2, 4, 2, 1);
