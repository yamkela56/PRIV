create database PRIVDB;

-- 1. Users Table 
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName VARCHAR(100) NOT NULL,
    Username VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Bio VARCHAR(255) NULL,
    IsDiscoverable BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);

-- Case-insensitive index for username lookups (@username)
CREATE UNIQUE INDEX IX_Users_Username_Lower ON Users(Username);

-- 2. Connections Table (Access Requests)
CREATE TABLE Connections (
    ConnectionId INT PRIMARY KEY IDENTITY(1,1),
    RequesterId INT FOREIGN KEY REFERENCES Users(UserId),
    TargetId INT FOREIGN KEY REFERENCES Users(UserId),
    Status VARCHAR(20) NOT NULL CHECK (Status IN ('Pending', 'Approved', 'Declined')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_Connection UNIQUE(RequesterId, TargetId)
);

-- 3. Blocked Times
CREATE TABLE BlockedTimes (
    BlockId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    Note VARCHAR(100) NULL
);

-- 4. Bookings Table 
CREATE TABLE Bookings (
    BookingId INT PRIMARY KEY IDENTITY(1,1),
    RequesterId INT FOREIGN KEY REFERENCES Users(UserId),
    HostId INT FOREIGN KEY REFERENCES Users(UserId),
    BookingType VARCHAR(50) NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    Location1 VARCHAR(150) NOT NULL,
    Location2 VARCHAR(150) NULL,
    Location3 VARCHAR(150) NULL,
    ConfirmedLocation VARCHAR(150) NULL,
    Status VARCHAR(20) NOT NULL CHECK (Status IN ('Pending', 'Approved', 'Declined', 'Cancelled', 'Completed')),
    DeclineCancelReason VARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);