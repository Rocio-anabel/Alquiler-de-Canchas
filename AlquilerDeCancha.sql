-- =====================================================
-- Alquiler de Canchas - Script de base de datos (MySQL)
-- Ejecutar completo en phpMyAdmin (pestaña SQL)
-- =====================================================
CREATE DATABASE IF NOT EXISTS alquiler_canchas
  CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE alquiler_canchas;
ALTER TABLE usuario MODIFY estado tinyint(1) NOT NULL DEFAULT 1;

-- ---------------------- USUARIO ----------------------
CREATE TABLE IF NOT EXISTS `usuario` (
  `ID_usuario` int(11) NOT NULL AUTO_INCREMENT,
  `email` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `avatar` varchar(255) DEFAULT NULL,
  `nombre` varchar(30) NOT NULL,
  `apellido` varchar(30) NOT NULL,
  `dni` varchar(15) NOT NULL,
  `telefono` varchar(20) DEFAULT NULL,
  `rol` enum('Empleado','Administrador') NOT NULL,
  `estado` tinyint(4) NOT NULL DEFAULT 1,
  PRIMARY KEY (`ID_usuario`),
  UNIQUE KEY `uk_usuario_email` (`email`),
  UNIQUE KEY `uk_usuario_dni` (`dni`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ---------------------- CLIENTE ----------------------
CREATE TABLE IF NOT EXISTS `alquiler_canchas`.`cliente` (
  `ID_cliente` INT NOT NULL AUTO_INCREMENT , 
  `nombre` VARCHAR(30) NOT NULL , 
  `apellido` VARCHAR(30) NOT NULL ,
  `email` VARCHAR(255) NOT NULL , 
  `telefono` VARCHAR(20) NOT NULL , 
  PRIMARY KEY (`ID_cliente`)) ENGINE = InnoDB;

------------------------- TIPO DE CANCHAS ----------------------
CREATE TABLE IF NOT EXISTS `alquiler_canchas`.`tipo_cancha` (
  `ID_tipo_cancha` INT NOT NULL AUTO_INCREMENT, 
  `nombre` VARCHAR(50) NOT NULL, 
  `estado` TINYINT NOT NULL, 
  PRIMARY KEY (`ID_tipo_cancha`)) ENGINE = InnoDB;

------------------------- CANCHA ----------------------
CREATE TABLE IF NOT EXISTS `alquiler_canchas`.`cancha` (
    `ID_cancha` INT NOT NULL AUTO_INCREMENT,
    `numero` INT NOT NULL,
    `tipo_superficie` VARCHAR(50) NOT NULL,
    `techada` TINYINT NOT NULL,
    `precio_por_hora` DOUBLE NOT NULL,
    `ID_tipo_cancha` INT NOT NULL,
    `estado` TINYINT NOT NULL,
    PRIMARY KEY (`ID_cancha`),
    
    CONSTRAINT `fk_cancha_tipo_cancha` 
        FOREIGN KEY (`ID_tipo_cancha`) 
        REFERENCES `tipo_cancha`(`ID_tipo_cancha`) 
        ON DELETE RESTRICT 
        ON UPDATE RESTRICT
) ENGINE = InnoDB;

-- Datos de prueba (contraseñas hasheadas con PasswordHasher de ASP.NET Core)
--   admin@canchas.com     / Admin123!
--   empleado@canchas.com  / Empleado123!
INSERT INTO `usuario` (`email`, `password`, `avatar`, `nombre`, `apellido`, `dni`, `telefono`, `rol`, `estado`) VALUES
('admin@canchas.com', 'AQAAAAEAACcQAAAAENnZaJzxsBByANO2o7Qbse+RVS5Dege6yWqYRvhgCVZcZL3+Y94/QNGs0SgAPKISaw==', NULL, 'Admin', 'General', '30111222', '2664000001', 'Administrador', 1),
('empleado@canchas.com', 'AQAAAAEAACcQAAAAEJ0gEKH6HYgQWf5clLEsYK0AGiMfGHaJFpjCPLUR5hL4QvUk4jAF0NR7OPgMUE061Q==', NULL, 'Juan', 'Pérez', '31222333', '2664000002', 'Empleado', 1);
