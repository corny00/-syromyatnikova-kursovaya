CREATE DATABASE GBDD_BD
USE GBDD_BD

CREATE TABLE Roles (
    RoleId INT PRIMARY KEY,
    RoleName NVARCHAR(30) UNIQUE NOT NULL
)

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    RoleId INT FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    
)

CREATE TABLE VehicleCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(5) UNIQUE NOT NULL, 
    Name NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1
)


CREATE TABLE CarMakes (
    MakeId INT IDENTITY(1,1) PRIMARY KEY,
    MakeName NVARCHAR(50) UNIQUE NOT NULL,
    IsActive BIT DEFAULT 1
)

CREATE TABLE CarModels (
    ModelId INT IDENTITY(1,1) PRIMARY KEY,
    MakeId INT NOT NULL FOREIGN KEY (MakeId) REFERENCES CarMakes(MakeId),
    ModelName NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    UNIQUE (MakeId, ModelName)
)

CREATE TABLE CarColors (
    ColorId INT IDENTITY(1,1) PRIMARY KEY,
    ColorName NVARCHAR(30) UNIQUE NOT NULL,
    HexCode NVARCHAR(7) NULL,
    IsActive BIT DEFAULT 1
)

CREATE TABLE TowTrucks (
    TowTruckId INT IDENTITY(1,1) PRIMARY KEY,
    PlateNumber NVARCHAR(20) UNIQUE NOT NULL,
    DriverName NVARCHAR(100) NOT NULL,
    CompanyName NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    IsActive BIT DEFAULT 1
)

CREATE TABLE VehicleOwners (
    OwnerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    PassportSeriesNumber NVARCHAR(20) NULL,
    DriverLicenseNumber NVARCHAR(20) NULL,
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME()
)

CREATE TABLE DamageZones (
    ZoneId INT IDENTITY(1,1) PRIMARY KEY,
    ZoneCode NVARCHAR(20) UNIQUE NOT NULL, 
    DisplayName NVARCHAR(50) NOT NULL
)

CREATE TABLE DamageTypes (
    TypeId INT IDENTITY(1,1) PRIMARY KEY,
    TypeCode NVARCHAR(20) UNIQUE NOT NULL,
    DisplayName NVARCHAR(50) NOT NULL
)


CREATE TABLE ChecklistDefinitions (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(50) UNIQUE NOT NULL,
    IsDefault BIT DEFAULT 1 
)


CREATE TABLE PaymentMethods (
    MethodId INT IDENTITY(1,1) PRIMARY KEY,
    MethodName NVARCHAR(100) UNIQUE NOT NULL,
    IsActive BIT DEFAULT 1
)


CREATE TABLE Tariffs (
    TariffId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT FOREIGN KEY (CategoryId) REFERENCES VehicleCategories(CategoryId) NOT NULL,
    TowCost DECIMAL(10,2) NOT NULL,
    HourlyRate DECIMAL(10,2) NOT NULL,
    DailyCap DECIMAL(10,2) NULL,
    ValidFrom DATE NOT NULL,
    ValidTo DATE NULL,
    IsActive BIT DEFAULT 1,
    CHECK (HourlyRate >= 0 AND TowCost >= 0)
)

CREATE TABLE Statuses ( 
    StatusId INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL UNIQUE
)

CREATE TABLE EvacuationRegistry (
    RegistryId INT IDENTITY(1,1) PRIMARY KEY,
    LicensePlate NVARCHAR(20) NOT NULL,
    VIN NVARCHAR(17) NOT NULL,
    MakeId INT FOREIGN KEY REFERENCES CarMakes(MakeId) NOT NULL,
    ModelId INT FOREIGN KEY REFERENCES CarModels(ModelId) NOT NULL,
    ColorId INT FOREIGN KEY REFERENCES CarColors(ColorId) NOT NULL,
    CategoryId INT FOREIGN KEY REFERENCES VehicleCategories(CategoryId) NOT NULL,
    ProtocolNumber NVARCHAR(50) NOT NULL,
    InspectorUserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    LegalArticle NVARCHAR(50) NOT NULL,
    TowTruckId INT NULL FOREIGN KEY REFERENCES TowTrucks(TowTruckId),
    OwnerId INT NULL FOREIGN KEY REFERENCES VehicleOwners(OwnerId),
    IntakeDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ReleaseDate DATETIME2 NULL,
    StatusId INT  FOREIGN KEY REFERENCES Statuses(StatusId) DEFAULT 1,
    CreatedByUserId INT NULL FOREIGN KEY REFERENCES Users(UserId),
    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    UNIQUE (LicensePlate, IntakeDate)
)

CREATE TABLE Damages (
    DamageId INT IDENTITY(1,1) PRIMARY KEY,
    RegistryId INT NOT NULL,
    ZoneId INT NOT NULL,
    TypeId INT NOT NULL,
    Description NVARCHAR(200) NULL,
    IsPreExisting BIT DEFAULT 1,
    FOREIGN KEY (RegistryId) REFERENCES EvacuationRegistry(RegistryId) ON DELETE CASCADE,
    FOREIGN KEY (ZoneId) REFERENCES DamageZones(ZoneId),
    FOREIGN KEY (TypeId) REFERENCES DamageTypes(TypeId)
)

CREATE TABLE ImpoundChecklist (
    ChecklistId INT IDENTITY(1,1) PRIMARY KEY,
    RegistryId INT NOT NULL,
    ItemId INT NOT NULL,
    IsPresent BIT DEFAULT 0,
    FOREIGN KEY (RegistryId) REFERENCES EvacuationRegistry(RegistryId) ON DELETE CASCADE,
    FOREIGN KEY (ItemId) REFERENCES ChecklistDefinitions(ItemId),
    UNIQUE (RegistryId, ItemId)
)

CREATE TABLE ReleaseVerifications (
    VerificationId INT IDENTITY(1,1) PRIMARY KEY,
    RegistryId INT UNIQUE NOT NULL,
    GibddPermissionReceived BIT DEFAULT 0,
    IdentityVerified BIT DEFAULT 0,
    DocumentsChecked BIT DEFAULT 0,
    VerifiedByUserId INT NULL,
    VerifiedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RegistryId) REFERENCES EvacuationRegistry(RegistryId),
    FOREIGN KEY (VerifiedByUserId) REFERENCES Users(UserId)
)

CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    RegistryId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    MethodId INT NOT NULL,
    PaymentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ReceiptNumber NVARCHAR(50) UNIQUE NULL,
    FOREIGN KEY (RegistryId) REFERENCES EvacuationRegistry(RegistryId),
    FOREIGN KEY (MethodId) REFERENCES PaymentMethods(MethodId),
    CHECK (Amount > 0)
)

CREATE TABLE VehiclePhotos (
    PhotoId INT IDENTITY(1,1) PRIMARY KEY,
    RegistryId INT NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    UploadDate DATETIME2 DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (RegistryId) REFERENCES EvacuationRegistry(RegistryId) ON DELETE CASCADE
)


INSERT INTO Roles VALUES (1, N'Admin'), (2, N'Dispatcher'), (3, N'Inspector');

INSERT INTO Statuses (StatusName) VALUES 
(N'На участке'), 
(N'Освобожден'), 
(N'Архив');

INSERT INTO VehicleCategories (Code, Name) VALUES 
(N'A', N'Мотоциклы'), 
(N'B', N'Легковые'), 
(N'C', N'Грузовые');

INSERT INTO DamageZones VALUES 
('FL', N'Перед левый'), ('FR', N'Перед правый'), 
('RL', N'Зад левый'), ('RR', N'Зад правый'), 
('RF', N'Крыша'), ('RT', N'Пороги'), ('RB', N'Бампер зад');

INSERT INTO DamageTypes VALUES 
('SCR', N'Царапина'), ('DNT', N'Вмятина'), 
('CRK', N'Трещина'), ('BRK', N'Скол');

INSERT INTO ChecklistDefinitions (ItemName) VALUES 
(N'Радио/Мультимедиа'), 
(N'Запасное колесо'), 
(N'Детское кресло'), 
(N'Аптечка'), 
(N'Огнетушитель'), 
(N'Домкрат'), 
(N'Знак аварийной остановки');

INSERT INTO PaymentMethods (MethodName ) VALUES (N'Наличные'), (N'Банковская карта'), (N'Безналичный перевод');

INSERT INTO Users (Username, PasswordHash, FullName, RoleId, IsActive) VALUES 
('admin', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'Сидоров Иван Петрович', 1, 1),

('dispatcher', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'Иванов Иван Иванович', 2, 1),

('inspector', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'Петров Петр Петрович', 3, 1);

INSERT INTO CarMakes (MakeName) VALUES
(N'Lada'), (N'Toyota'), (N'Kia'), (N'BMW'), 
(N'Mercedes-Benz'), (N'Kamaz'), (N'GAZ');

INSERT INTO CarModels (MakeId, ModelName) VALUES
(1, N'Vesta'), (1, N'Granta'), (1, N'Niva Travel'),
(2, N'Camry'), (2, N'Corolla'), (2, N'RAV4'),
(3, N'Rio'), (3, N'Ceed'), (3, N'Sportage'),
(4, N'X5'), (4, N'3 Series'), (4, N'5 Series'),
(5, N'E-Class'), (5, N'C-Class'),
(6, N'5490'), (6, N'6520'),
(7, N'ГАЗель NEXT'), (7, N'Соболь');

INSERT INTO CarColors (ColorName, HexCode) VALUES
(N'Чёрный', '#111111'), (N'Белый', '#FFFFFF'), (N'Серебристый', '#C0C0C0'),
(N'Синий', '#1E3A8A'), (N'Красный', '#DC2626'), (N'Серый', '#6B7280'),
(N'Зелёный', '#166534'), (N'Жёлтый', '#FACC15');

INSERT INTO TowTrucks (PlateNumber, DriverName, CompanyName, Phone) VALUES
(N'А111АА77', N'Смирнов Алексей Владимирович', N'ООО "Эвакуатор-Сервис"', N'+7 (999) 111-22-33'),
(N'В222ВВ77', N'Козлов Дмитрий Сергеевич', N'ИП Козлов Д.С.', N'+7 (999) 444-55-66'),
(N'М333ММ77', N'Новиков Игорь Андреевич', N'ООО "СпецТранс"', N'+7 (999) 777-88-99');


INSERT INTO VehicleOwners (FullName, Phone, PassportSeriesNumber, DriverLicenseNumber) VALUES
(N'Кузнецов Михаил Игоревич', N'+7 (903) 123-45-67', N'4510 123456', N'7710 123456'),
(N'Васильева Анна Петровна', N'+7 (916) 987-65-43', N'4515 654321', N'7715 654321'),
(N'Морозов Сергей Дмитриевич', N'+7 (925) 555-44-33', N'4520 112233', N'7720 112233'),
(N'Лебедева Елена Викторовна', N'+7 (905) 222-11-00', N'4525 445566', N'7725 445566'),
(N'ООО "ТрансЛогистик"', N'+7 (495) 100-20-30', N'ЮЛ-00123', N'Вод. уд. 7701 998877');

INSERT INTO Tariffs (CategoryId, TowCost, HourlyRate, DailyCap, ValidFrom) VALUES
(1, 3000.00, 50.00, 1000.00, '2024-01-01'), 
(2, 5000.00, 100.00, 2000.00, '2024-01-01'), 
(3, 8000.00, 200.00, 4000.00, '2024-01-01');

DECLARE @Today DATETIME = GETDATE();

--INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, StatusId, CreatedByUserId)
--VALUES (N'А123БВ77', N'XTA21150081234567', 1, 1, 1, 2, N'77-УК-001234', 3, N'12.35 ч.1 КоАП РФ', 1, 1, DATEADD(day, -5, @Today), 1, 2);

---- 2. Toyota Camry (35 дней назад) -> НА УЧАСТКЕ (ДОЛЖНИК!)
--INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, StatusId, CreatedByUserId)
--VALUES (N'К456МН77', N'JTNBV98K101234567', 2, 4, 2, 2, N'77-УК-005678', 3, N'12.35 ч.1 КоАП РФ', 2, 2, DATEADD(day, -35, @Today), 1, 2);

---- 3. Kia Rio (20 дней назад, выдана 2 дня назад) -> ОСВОБОЖДЕН
--INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, ReleaseDate, StatusId, CreatedByUserId)
--VALUES (N'О789РС77', N'Z94CT41DBM0123456', 3, 7, 3, 2, N'77-УК-009012', 3, N'12.35 ч.1 КоАП РФ', 3, 3, DATEADD(day, -20, @Today), DATEADD(day, -2, @Today), 2, 2);

-- 4. BMW X5 (10 дней назад) -> На участке
INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, StatusId, CreatedByUserId)
VALUES (N'У012ТУ77', N'WBAKB210500123456', 4, 10, 4, 2, N'77-УК-003456', 3, N'12.35 ч.1 КоАП РФ', 1, 4, DATEADD(day, -10, @Today), 1, 2);

-- 5. Kamaz 6520 (45 дней назад) -> НА УЧАСТКЕ (ДОЛЖНИК!)
INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, StatusId, CreatedByUserId)
VALUES (N'Х345ЦЧ77', N'KMAJH811000123456', 6, 16, 5, 3, N'77-УК-007890', 3, N'12.35 ч.2 КоАП РФ', 2, 5, DATEADD(day, -45, @Today), 1, 2);

-- 6. Mercedes E-Class (60 дней назад) -> АРХИВ
INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, StatusId, CreatedByUserId)
VALUES (N'Ш678ЩЭ77', N'WDD2120501A123456', 5, 13, 6, 2, N'77-УК-001122', 3, N'12.35 ч.1 КоАП РФ', 3, 2, DATEADD(day, -60, @Today), 3, 2);

-- 7. Lada Granta (вчера) -> На участке
INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, StatusId, CreatedByUserId)
VALUES (N'Ю901ЯА77', N'XTA21900082345678', 1, 2, 7, 2, N'77-УК-004455', 3, N'12.35 ч.1 КоАП РФ', 1, 1, DATEADD(day, -1, @Today), 1, 2);

-- 8. ГАЗель NEXT (15 дней назад) -> На участке
INSERT INTO EvacuationRegistry (LicensePlate, VIN, MakeId, ModelId, ColorId, CategoryId, ProtocolNumber, InspectorUserId, LegalArticle, TowTruckId, OwnerId, IntakeDate, StatusId, CreatedByUserId)
VALUES (N'АБ12ВГ77', N'XTH330230G1234567', 7, 17, 8, 3, N'77-УК-006677', 3, N'12.35 ч.2 КоАП РФ', 2, 5, DATEADD(day, -15, @Today), 1, 2);

