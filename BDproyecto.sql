-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: proyecto
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `bitacora`
--

DROP TABLE IF EXISTS `bitacora`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bitacora` (
  `idbitacora` int NOT NULL AUTO_INCREMENT,
  `idusuario` int DEFAULT NULL,
  `fechaentrada` datetime NOT NULL,
  `fechasalida` datetime DEFAULT NULL,
  PRIMARY KEY (`idbitacora`),
  KEY `idusuario` (`idusuario`),
  CONSTRAINT `idusuario` FOREIGN KEY (`idusuario`) REFERENCES `usuario` (`idusuario`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bitacora`
--

LOCK TABLES `bitacora` WRITE;
/*!40000 ALTER TABLE `bitacora` DISABLE KEYS */;
/*!40000 ALTER TABLE `bitacora` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `categoria`
--

DROP TABLE IF EXISTS `categoria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categoria` (
  `Cacodigo` int NOT NULL,
  `Catipo` varchar(20) NOT NULL,
  PRIMARY KEY (`Cacodigo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categoria`
--

LOCK TABLES `categoria` WRITE;
/*!40000 ALTER TABLE `categoria` DISABLE KEYS */;
INSERT INTO `categoria` VALUES (1,'Electronica'),(2,'Ropa'),(3,'Hogar'),(4,'Deportes'),(5,'Juguetes'),(6,'Belleza'),(7,'Alimentos'),(8,'Libros'),(9,'Musica'),(10,'Peliculas');
/*!40000 ALTER TABLE `categoria` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `producto`
--

DROP TABLE IF EXISTS `producto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `producto` (
  `prid` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(30) DEFAULT NULL,
  `descripcion` varchar(50) DEFAULT NULL,
  `categoria` int NOT NULL,
  PRIMARY KEY (`prid`),
  KEY `categoria_idx` (`categoria`),
  CONSTRAINT `categoria` FOREIGN KEY (`categoria`) REFERENCES `categoria` (`Cacodigo`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `producto`
--

LOCK TABLES `producto` WRITE;
/*!40000 ALTER TABLE `producto` DISABLE KEYS */;
INSERT INTO `producto` VALUES (1,'Laptop Lenovo','Portatil Ryzen 7 16GB RAM',1),(2,'Smartphone Samsung','Galaxy con camara 50MP',1),(3,'Camiseta Deportiva','Camiseta de secado rapido',2),(4,'Jean Clasico','Pantalon de mezclilla azul',2),(5,'Sofa 3 Puestos','Sofa moderno color gris',3),(6,'Licuadora Oster','Licuadora de 3 velocidades',3),(7,'Balon Futbol','Balon profesional tamaño 5',4),(8,'Bicicleta MTB','Bicicleta montañera 29 pulgadas',4),(9,'Muñeca Barbie','Muñeca articulada',5),(10,'Rompecabezas 1000','Puzzle de 1000 piezas',5),(11,'Perfume Floral','Fragancia femenina 100ml',6),(12,'Kit Maquillaje','Set completo maquillaje',6),(13,'Arroz Premium','Bolsa de arroz 1kg',7),(14,'Chocolate Negro','Tableta chocolate 70%',7),(15,'Libro Java','Aprende Java desde cero',8),(16,'Libro SQL','Guia practica bases datos',8),(17,'Album Rock','Coleccion clasicos rock',9),(18,'Guitarra Acustica','Guitarra principiantes',9),(19,'Pelicula Accion','Blu-ray accion',10),(20,'Pelicula SciFi','Blu-ray ciencia ficcion',10);
/*!40000 ALTER TABLE `producto` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `productosofertados`
--

DROP TABLE IF EXISTS `productosofertados`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `productosofertados` (
  `proveedorid` int NOT NULL,
  `producto` int NOT NULL,
  `precio` decimal(12,2) NOT NULL,
  `tiempoentrega` int NOT NULL,
  KEY `idprovedor_idx` (`proveedorid`),
  KEY `idproducto_idx` (`producto`),
  CONSTRAINT `idproducto` FOREIGN KEY (`producto`) REFERENCES `producto` (`prid`),
  CONSTRAINT `idprovedor` FOREIGN KEY (`proveedorid`) REFERENCES `usuario` (`idusuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `productosofertados`
--

LOCK TABLES `productosofertados` WRITE;
/*!40000 ALTER TABLE `productosofertados` DISABLE KEYS */;
/*!40000 ALTER TABLE `productosofertados` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rolusuario`
--

DROP TABLE IF EXISTS `rolusuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rolusuario` (
  `idrol` int NOT NULL,
  `rol` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`idrol`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rolusuario`
--

LOCK TABLES `rolusuario` WRITE;
/*!40000 ALTER TABLE `rolusuario` DISABLE KEYS */;
INSERT INTO `rolusuario` VALUES (1,'Administrador'),(2,'proveedor'),(3,'vendedor');
/*!40000 ALTER TABLE `rolusuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuario`
--

DROP TABLE IF EXISTS `usuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
  CONSTRAINT `idrol` FOREIGN KEY (`rolusuario`) REFERENCES `rolusuario` (`idrol`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario`
--

LOCK TABLES `usuario` WRITE;
/*!40000 ALTER TABLE `usuario` DISABLE KEYS */;
INSERT INTO `usuario` VALUES (1,'admin','casa','3124253567','12345','s@email.com','1970-09-12','M',1,'cocacola','circasia'),(2,'porvee','apartamento','314567890','2345','d@email.com','2000-10-11','M',2,'exito','armenia'),(3,'vendedor','finca','314567890','3456','v@email.com','1990-12-03','F',3,'D1','armenia');
/*!40000 ALTER TABLE `usuario` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-07 22:08:02
