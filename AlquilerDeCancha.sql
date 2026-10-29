-- =====================================================
-- Alquiler de Canchas - Script de base de datos (MySQL)
-- Ejecutar completo en phpMyAdmin (pestaña SQL)
-- =====================================================
CREATE DATABASE IF NOT EXISTS alquiler_canchas
  CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE alquiler_canchas;

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
  `estado` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`ID_usuario`),
  UNIQUE KEY `uk_usuario_email` (`email`),
  UNIQUE KEY `uk_usuario_dni` (`dni`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ---------------------- CLIENTE ----------------------
CREATE TABLE IF NOT EXISTS `cliente` (
  `ID_cliente` INT NOT NULL AUTO_INCREMENT,
  `nombre` VARCHAR(30) NOT NULL,
  `apellido` VARCHAR(30) NOT NULL,
  `email` VARCHAR(255) NOT NULL,
  `telefono` VARCHAR(20) NOT NULL,
  PRIMARY KEY (`ID_cliente`)
) ENGINE=InnoDB;

-- ---------------------- TIPO DE CANCHAS ----------------------
CREATE TABLE IF NOT EXISTS `tipo_cancha` (
  `ID_tipo_cancha` INT NOT NULL AUTO_INCREMENT,
  `nombre` VARCHAR(50) NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`ID_tipo_cancha`)
) ENGINE=InnoDB;

-- ---------------------- CANCHA ----------------------
CREATE TABLE IF NOT EXISTS `cancha` (
  `ID_cancha` INT NOT NULL AUTO_INCREMENT,
  `numero` INT NOT NULL,
  `tipo_superficie` VARCHAR(50) NOT NULL,
  `techada` TINYINT NOT NULL DEFAULT 0,
  `precio_por_hora` DOUBLE NOT NULL,
  `ID_tipo_cancha` INT NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`ID_cancha`),
  CONSTRAINT `fk_cancha_tipo_cancha`
    FOREIGN KEY (`ID_tipo_cancha`)
    REFERENCES `tipo_cancha`(`ID_tipo_cancha`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- ---------------------- IMAGEN ----------------------
CREATE TABLE IF NOT EXISTS `imagen` (
  `ID_imagen` INT NOT NULL AUTO_INCREMENT,
  `ID_cancha` INT NOT NULL,
  `url` VARCHAR(255) NOT NULL,
  `fecha_subida` DATETIME NOT NULL,
  PRIMARY KEY (`ID_imagen`),
  CONSTRAINT `fk_imagen_cancha`
    FOREIGN KEY (`ID_cancha`)
    REFERENCES `cancha`(`ID_cancha`)
    ON DELETE RESTRICT
    ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- ---------------------- TURNO -----------------------
CREATE TABLE IF NOT EXISTS `alquiler_canchas`.`turno` (
    `ID_turno` INT NOT NULL AUTO_INCREMENT,
    `ID_cancha` INT NOT NULL,
    `ID_cliente` INT NOT NULL,
    `ID_usuario` INT NOT NULL,
    `fecha` DATE NOT NULL,
    `hora_inicio` TIME NOT NULL,
    `hora_fin` TIME NOT NULL,
    `estado` ENUM('Confirmado','Finalizado','Cancelado','Ausente') NOT NULL DEFAULT 'Confirmado',
    PRIMARY KEY (`ID_turno`),
    
    CONSTRAINT `fk_turno_cancha` 
        FOREIGN KEY (`ID_cancha`) 
        REFERENCES `cancha`(`ID_cancha`) 
        ON DELETE RESTRICT ON UPDATE RESTRICT,
        
    CONSTRAINT `fk_turno_cliente` 
        FOREIGN KEY (`ID_cliente`) 
        REFERENCES `cliente`(`ID_cliente`) 
        ON DELETE RESTRICT ON UPDATE RESTRICT,
        
    CONSTRAINT `fk_turno_usuario` 
        FOREIGN KEY (`ID_usuario`) 
        REFERENCES `usuario`(`ID_usuario`) 
        ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB;

-- ---------------------- PAGO -----------------------
-- tipo_transaccion (enum TipoTransaccion en C#): 0 = Seña, 1 = Pago total, 2 = Reembolso
CREATE TABLE IF NOT EXISTS `pago` (
  `ID_pago` INT NOT NULL AUTO_INCREMENT,
  `ID_turno` INT NOT NULL,
  `ID_usuario` INT NOT NULL,
  `monto` DECIMAL(10,2) NOT NULL,
  `fecha_pago` DATE NOT NULL,
  `metodo_pago` VARCHAR(30) NOT NULL,
  `tipo_transaccion` INT NOT NULL DEFAULT 1,
  PRIMARY KEY (`ID_pago`),
  CONSTRAINT `fk_pago_turno`
    FOREIGN KEY (`ID_turno`) REFERENCES `turno`(`ID_turno`)
    ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `fk_pago_usuario`
    FOREIGN KEY (`ID_usuario`) REFERENCES `usuario`(`ID_usuario`)
    ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- =====================================================
-- DATOS DE PRUEBA
-- (contraseñas hasheadas con PasswordHasher de ASP.NET Core)
--   admin@canchas.com     / Admin123!
--   empleado@canchas.com  / Empleado123!
-- INSERT IGNORE: si el script se ejecuta dos veces, no falla por email/DNI repetidos
-- =====================================================
INSERT IGNORE INTO `usuario` (`email`, `password`, `avatar`, `nombre`, `apellido`, `dni`, `telefono`, `rol`, `estado`) VALUES
('admin@canchas.com', 'AQAAAAEAACcQAAAAENnZaJzxsBByANO2o7Qbse+RVS5Dege6yWqYRvhgCVZcZL3+Y94/QNGs0SgAPKISaw==', NULL, 'Admin', 'General', '30111222', '2664000001', 'Administrador', 1),
('empleado@canchas.com', 'AQAAAAEAACcQAAAAEJ0gEKH6HYgQWf5clLEsYK0AGiMfGHaJFpjCPLUR5hL4QvUk4jAF0NR7OPgMUE061Q==', NULL, 'Juan', 'Pérez', '31222333', '2664000002', 'Empleado', 1);
