-- ============================================================
-- BASE DE DATOS: proyecto
-- Descripción: Creación de tablas y carga de datos iniciales
-- Motor: MySQL / InnoDB
-- ============================================================

CREATE DATABASE IF NOT EXISTS `proyecto`
DEFAULT CHARACTER SET utf8mb4;

USE `proyecto`;

-- ============================================================
-- 1. CONFIGURACIÓN INICIAL
-- ============================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- 2. ELIMINAR TABLAS EXISTENTES
-- Se eliminan primero las tablas que dependen de otras.
-- ============================================================

DROP TABLE IF EXISTS `productosofertados`;
DROP TABLE IF EXISTS `producto`;
DROP TABLE IF EXISTS `usuario`;
DROP TABLE IF EXISTS `categoria`;
DROP TABLE IF EXISTS `rolusuario`;

-- ============================================================
-- 3. TABLA: rolusuario
-- ============================================================

CREATE TABLE `rolusuario` (
  `idrol` int NOT NULL,
  `rol` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`idrol`)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_0900_ai_ci;

-- Datos de rolusuario

INSERT INTO `rolusuario` (`idrol`, `rol`) VALUES
(1, 'Administrador'),
(2, 'proveedor'),
(3, 'vendedor');

-- ============================================================
-- 4. TABLA: categoria
-- ============================================================

CREATE TABLE `categoria` (
  `Cacodigo` int NOT NULL,
  `nombre` varchar(30) DEFAULT NULL,
  PRIMARY KEY (`Cacodigo`)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_0900_ai_ci;

-- Datos de categoria

INSERT INTO `categoria` VALUES
(1, 'Electronica'),
(2, 'Ropa'),
(3, 'Hogar'),
(4, 'Deportes'),
(5, 'Juguetes'),
(6, 'Belleza'),
(7, 'Alimentos'),
(8, 'Libros'),
(9, 'Musica'),
(10, 'Peliculas');

-- ============================================================
-- 5. TABLA: usuario
-- ============================================================

CREATE TABLE `usuario` (
  `idusuario` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(30) DEFAULT NULL,
  `direccion` varchar(30) DEFAULT NULL,
  `telefono` varchar(10) DEFAULT NULL,
  `cedula` varchar(30) DEFAULT NULL,
  `correo` varchar(25) DEFAULT NULL,
  `fechanacimiento` date DEFAULT NULL,
  `genero` varchar(1) DEFAULT NULL,
  `rolusuario` int NOT NULL,
  `nombreempresa` varchar(20) DEFAULT NULL,
  `ciudad` varchar(30) DEFAULT NULL,
  PRIMARY KEY (`idusuario`),
  KEY `idrol_idx` (`rolusuario`),
  CONSTRAINT `idrol`
    FOREIGN KEY (`rolusuario`)
    REFERENCES `rolusuario` (`idrol`)
) ENGINE=InnoDB
AUTO_INCREMENT=4
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_0900_ai_ci;

-- Datos de usuario

INSERT INTO `usuario` VALUES
(1, 'admin', 'casa', '3124253567', '12345',
 's@email.com', '1970-09-12', 'M', 1, 'cocacola', 'circasia'),
(2, 'porvee', 'apartamento', '314567890', '2345',
 'd@email.com', '2000-10-11', 'M', 2, 'exito', 'armenia'),
(3, 'vendedor', 'finca', '314567890', '3456',
 'v@email.com', '1990-12-03', 'F', 3, 'D1', 'armenia');

-- ============================================================
-- 6. TABLA: producto
-- ============================================================

CREATE TABLE `producto` (
  `Prid` int NOT NULL,
  `nombre` varchar(30) DEFAULT NULL,
  `descripcion` varchar(50) DEFAULT NULL,
  `categoria` int DEFAULT NULL,
  PRIMARY KEY (`Prid`),
  KEY `categoria_idx` (`categoria`),
  CONSTRAINT `categoria`
    FOREIGN KEY (`categoria`)
    REFERENCES `categoria` (`Cacodigo`)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_0900_ai_ci;

-- Datos de producto

INSERT INTO `producto` VALUES
(1, 'Laptop Lenovo', 'Portatil Ryzen 7 16GB RAM', 1),
(2, 'Smartphone Samsung', 'Galaxy con camara 50MP', 1),
(3, 'Camiseta Deportiva', 'Camiseta de secado rapido', 2),
(4, 'Jean Clasico', 'Pantalon de mezclilla azul', 2),
(5, 'Sofa 3 Puestos', 'Sofa moderno color gris', 3),
(6, 'Licuadora Oster', 'Licuadora de 3 velocidades', 3),
(7, 'Balon Futbol', 'Balon profesional tamaño 5', 4),
(8, 'Bicicleta MTB', 'Bicicleta montañera 29 pulgadas', 4),
(9, 'Muñeca Barbie', 'Muñeca articulada', 5),
(10, 'Rompecabezas 1000', 'Puzzle de 1000 piezas', 5),
(11, 'Perfume Floral', 'Fragancia femenina 100ml', 6),
(12, 'Kit Maquillaje', 'Set completo maquillaje', 6),
(13, 'Arroz Premium', 'Bolsa de arroz 1kg', 7),
(14, 'Chocolate Negro', 'Tableta chocolate 70%', 7),
(15, 'Libro Java', 'Aprende Java desde cero', 8),
(16, 'Libro SQL', 'Guia practica bases datos', 8),
(17, 'Album Rock', 'Coleccion clasicos rock', 9),
(18, 'Guitarra Acustica', 'Guitarra principiantes', 9),
(19, 'Pelicula Accion', 'Blu-ray accion', 10),
(20, 'Pelicula SciFi', 'Blu-ray ciencia ficcion', 10);

-- ============================================================
-- 7. TABLA: productosofertados
-- ============================================================

CREATE TABLE `productosofertados` (
  `proveedorid` int NOT NULL,
  `producto` int DEFAULT NULL,
  `precio` decimal(12,2) DEFAULT NULL,
  `tiempoentrega` int DEFAULT NULL,
  KEY `idproveedor_idx` (`proveedorid`),
  KEY `idproducto_idx` (`producto`),
  CONSTRAINT `idproducto`
    FOREIGN KEY (`producto`)
    REFERENCES `producto` (`Prid`),
  CONSTRAINT `idproveedor`
    FOREIGN KEY (`proveedorid`)
    REFERENCES `usuario` (`idusuario`)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_0900_ai_ci;

-- No existen registros iniciales en productosofertados.

-- ============================================================
-- 8. RESTAURAR CONFIGURACIÓN
-- ============================================================

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- FIN DEL SCRIPT
-- ============================================================
