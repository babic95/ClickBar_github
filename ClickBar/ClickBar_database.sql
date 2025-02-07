-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `mydb` DEFAULT CHARACTER SET utf8 ;
-- -----------------------------------------------------
-- Schema clickbar
-- -----------------------------------------------------
USE `mydb` ;

-- -----------------------------------------------------
-- Table `mydb`.`Cashier`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`Cashier` (
  `Id` VARCHAR(4) NOT NULL,
  `Type` INT NOT NULL,
  `Name` VARCHAR(45) NOT NULL,
  `Jmbg` VARCHAR(13) NULL,
  `City` VARCHAR(45) NULL,
  `Address` VARCHAR(65) NULL,
  `ContactNumber` VARCHAR(20) NULL,
  `Email` VARCHAR(60) NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`ItemGroup`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`ItemGroup` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`Item` (
  `Id` VARCHAR(15) NOT NULL,
  `IdItemGroup` INT NOT NULL,
  `Name` VARCHAR(500) NOT NULL,
  `UnitPrice` DECIMAL(2) NOT NULL,
  `Label` VARCHAR(1) NOT NULL,
  `JM` VARCHAR(5) NOT NULL,
  `TotalQuantity` DECIMAL(4) NOT NULL,
  `AlarmQuantity` DECIMAL(4) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `fk_Item_ItemGroup_idx` (`IdItemGroup` ASC) VISIBLE,
  CONSTRAINT `fk_Item_ItemGroup`
    FOREIGN KEY (`IdItemGroup`)
    REFERENCES `mydb`.`ItemGroup` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Invoice`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`Invoice` (
  `Id` VARCHAR(36) NOT NULL,
  `DateAndTimeOfIssue` DATETIME NULL,
  `Cashier` VARCHAR(45) NULL,
  `BuyerId` VARCHAR(45) NULL,
  `BuyerName` VARCHAR(75) NULL,
  `BuyerAddress` VARCHAR(45) NULL,
  `BuyerCostCenterId` VARCHAR(45) NULL,
  `InvoiceType` INT NULL,
  `TransactionType` INT NULL,
  `ReferentDocumentNumber` VARCHAR(50) NULL,
  `ReferentDocumentDT` DATETIME NULL,
  `InvoiceNumber` VARCHAR(50) NULL,
  `RequestedBy` VARCHAR(10) NULL,
  `InvoiceNumberResult` VARCHAR(50) NULL,
  `SdcDateTime` DATETIME NULL,
  `InvoiceCounter` VARCHAR(50) NULL,
  `InvoiceCounterExtension` VARCHAR(50) NULL,
  `SignedBy` VARCHAR(10) NULL,
  `EncryptedInternalData` VARCHAR(512) NULL,
  `Signature` VARCHAR(512) NULL,
  `TotalCounter` INT NULL,
  `TransactionTypeCounter` INT NULL,
  `TotalAmount` DECIMAL(4) NULL,
  `TaxGroupRevision` INT NULL,
  `BusinessName` VARCHAR(75) NULL,
  `Tin` VARCHAR(35) NULL,
  `LocationName` VARCHAR(95) NULL,
  `Address` VARCHAR(95) NULL,
  `District` VARCHAR(95) NULL,
  `Mrc` VARCHAR(55) NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`ItemInvoice`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`ItemInvoice` (
  `Id` INT NOT NULL,
  `InvoiceId` VARCHAR(36) NOT NULL,
  `ItemCode` VARCHAR(15) NULL,
  `Quantity` DECIMAL(4) NULL,
  `TotalAmout` DECIMAL(4) NULL,
  `Name` VARCHAR(500) NULL,
  `UnitPrice` DECIMAL(2) NULL,
  `Label` VARCHAR(1) NULL,
  PRIMARY KEY (`Id`, `InvoiceId`),
  INDEX `fk_ItemInvoice_Invoice1_idx` (`InvoiceId` ASC) VISIBLE,
  CONSTRAINT `fk_ItemInvoice_Invoice1`
    FOREIGN KEY (`InvoiceId`)
    REFERENCES `mydb`.`Invoice` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`PaymentInvoice`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`PaymentInvoice` (
  `PaymentType` INT NOT NULL,
  `InvoiceId` VARCHAR(36) NOT NULL,
  `Amout` DECIMAL(2) NULL,
  PRIMARY KEY (`PaymentType`, `InvoiceId`),
  INDEX `fk_PaymentInvoice_Invoice1_idx` (`InvoiceId` ASC) VISIBLE,
  CONSTRAINT `fk_PaymentInvoice_Invoice1`
    FOREIGN KEY (`InvoiceId`)
    REFERENCES `mydb`.`Invoice` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`TaxItemInvoice`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`TaxItemInvoice` (
  `Label` VARCHAR(1) NOT NULL,
  `Invoice_Id` VARCHAR(36) NOT NULL,
  `CategoryName` VARCHAR(45) NULL,
  `CategoryType` INT NULL,
  `Rate` DECIMAL(4) NULL,
  `Amount` DECIMAL(4) NULL,
  PRIMARY KEY (`Label`, `Invoice_Id`),
  INDEX `fk_TaxItemInvoice_Invoice1_idx` (`Invoice_Id` ASC) VISIBLE,
  CONSTRAINT `fk_TaxItemInvoice_Invoice1`
    FOREIGN KEY (`Invoice_Id`)
    REFERENCES `mydb`.`Invoice` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`SmartCard`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`SmartCard` (
  `Id` VARCHAR(15) NOT NULL,
  `CashierId` VARCHAR(4) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `fk_SmartCard_Cashier1_idx` (`CashierId` ASC) VISIBLE,
  CONSTRAINT `fk_SmartCard_Cashier1`
    FOREIGN KEY (`CashierId`)
    REFERENCES `mydb`.`Cashier` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Supplier`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`Supplier` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `PIB` VARCHAR(45) NULL,
  `Name` VARCHAR(95) NULL,
  `Address` VARCHAR(75) NULL,
  `ContractNumber` VARCHAR(20) NULL,
  `Email` VARCHAR(60) NULL,
  `City` VARCHAR(45) NULL,
  `MB` VARCHAR(45) NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Procurement`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`Procurement` (
  `Id` VARCHAR(36) NOT NULL,
  `SupplierId` INT NOT NULL,
  `ItemId` VARCHAR(15) NOT NULL,
  `DateProcurement` DATETIME NOT NULL,
  `Quantity` DECIMAL(4) NOT NULL,
  `UnitPrice` DECIMAL(2) NOT NULL,
  PRIMARY KEY (`Id`),
  INDEX `fk_Procurement_Supplier1_idx` (`SupplierId` ASC) VISIBLE,
  INDEX `fk_Procurement_Item1_idx` (`ItemId` ASC) VISIBLE,
  CONSTRAINT `fk_Procurement_Supplier1`
    FOREIGN KEY (`SupplierId`)
    REFERENCES `mydb`.`Supplier` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Procurement_Item1`
    FOREIGN KEY (`ItemId`)
    REFERENCES `mydb`.`Item` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`PartHall`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`PartHall` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(45) NOT NULL,
  `Image` VARCHAR(4096) NULL,
  PRIMARY KEY (`Id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`PaymentPlace`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`PaymentPlace` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `PartHallId` INT NOT NULL,
  `LeftCanvas` DECIMAL(2) NULL,
  `TopCanvas` DECIMAL(2) NULL,
  `Type` INT NULL,
  `Width` DECIMAL(2) NULL,
  `Height` DECIMAL(2) NULL,
  PRIMARY KEY (`Id`, `PartHallId`),
  INDEX `fk_Table_PartHall1_idx` (`PartHallId` ASC) VISIBLE,
  CONSTRAINT `fk_Table_PartHall1`
    FOREIGN KEY (`PartHallId`)
    REFERENCES `mydb`.`PartHall` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Order`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `mydb`.`Order` (
  `PaymentPlaceId` INT NOT NULL,
  `InvoiceId` VARCHAR(36) NOT NULL,
  `CashierId` VARCHAR(4) NOT NULL,
  PRIMARY KEY (`PaymentPlaceId`, `InvoiceId`, `CashierId`),
  INDEX `fk_Order_Invoice1_idx` (`InvoiceId` ASC) VISIBLE,
  INDEX `fk_Order_Cashier1_idx` (`CashierId` ASC) VISIBLE,
  CONSTRAINT `fk_Order_Table1`
    FOREIGN KEY (`PaymentPlaceId`)
    REFERENCES `mydb`.`PaymentPlace` (`PartHallId`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Order_Invoice1`
    FOREIGN KEY (`InvoiceId`)
    REFERENCES `mydb`.`Invoice` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Order_Cashier1`
    FOREIGN KEY (`CashierId`)
    REFERENCES `mydb`.`Cashier` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
