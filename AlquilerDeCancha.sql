CREATE TABLE `usuario` (
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