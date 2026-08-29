/* Users */
CREATE TABLE Users (
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    Name                    NVARCHAR(100)   NOT NULL,
    Username                NVARCHAR(50)    NOT NULL,
    UsernameNormalized      NVARCHAR(50)    NOT NULL,
    Email                   NVARCHAR(256)   NULL,
    PasswordHash            NVARCHAR(MAX)   NOT NULL,
    Bio                     NVARCHAR(500)   NULL,
    DiscoverableInSearch    BIT             NOT NULL DEFAULT 1,
    WorkDayStart            TIME            NOT NULL DEFAULT '08:00:00',
    WorkDayEnd              TIME            NOT NULL DEFAULT '20:00:00',
    SlotIncrementMinutes    INT             NOT NULL DEFAULT 60,
    CreatedAt               DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
 
    CONSTRAINT UQ_Users_UsernameNormalized UNIQUE (UsernameNormalized)
);

 
/* ConnectionRequests */
-- Status: 0 = Pending, 1 = Approved, 2 = Declined
CREATE TABLE ConnectionRequests (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RequesterId     INT NOT NULL,
    TargetId        INT NOT NULL,
    Status          INT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    RespondedAt     DATETIME2 NULL,
 
    CONSTRAINT FK_Connection_Requester FOREIGN KEY (RequesterId) REFERENCES Users(Id),
    CONSTRAINT FK_Connection_Target    FOREIGN KEY (TargetId)    REFERENCES Users(Id),
    CONSTRAINT UQ_Connection_Pair UNIQUE (RequesterId, TargetId)
);

 
/* BlockedTimes */
CREATE TABLE BlockedTimes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    Label           NVARCHAR(100) NOT NULL DEFAULT 'Busy',
    DayOfWeek       INT NULL,
    SpecificDate    DATE NULL,
    StartTime       TIME NOT NULL,
    EndTime         TIME NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
 
    CONSTRAINT FK_BlockedTimes_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CHK_BlockedTimes_OneModeOnly CHECK (
        (DayOfWeek IS NOT NULL AND SpecificDate IS NULL) OR
        (DayOfWeek IS NULL AND SpecificDate IS NOT NULL)
    ),
    CONSTRAINT CHK_BlockedTimes_TimeOrder CHECK (EndTime > StartTime)
);

 
/* BookingRequests */
CREATE TABLE BookingRequests (
    Id                          INT IDENTITY(1,1) PRIMARY KEY,
    RequesterId                 INT NOT NULL,
    TargetId                    INT NOT NULL,
    Type                        INT NOT NULL,
    CustomTypeLabel             NVARCHAR(100) NULL,
    Date                        DATE NOT NULL,
    StartTime                   TIME NOT NULL,
    EndTime                     TIME NOT NULL,
    Status                      INT NOT NULL DEFAULT 0,
    ConfirmedLocationOptionId   INT NULL,
    DeclineReason               NVARCHAR(500) NULL,
    CancelReason                NVARCHAR(500) NULL,
    CancelledByUserId           INT NULL,
    CreatedAt                   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    RespondedAt                 DATETIME2 NULL,
 
    CONSTRAINT FK_Booking_Requester FOREIGN KEY (RequesterId) REFERENCES Users(Id),
    CONSTRAINT FK_Booking_Target    FOREIGN KEY (TargetId)    REFERENCES Users(Id),
    CONSTRAINT CHK_Booking_TimeOrder CHECK (EndTime > StartTime)
);
 
/* BookingLocationOptions */
CREATE TABLE BookingLocationOptions (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    BookingRequestId    INT NOT NULL,
    OptionNumber        INT NOT NULL,  -- 1, 2, or 3
    Name                NVARCHAR(200) NOT NULL,
 
    CONSTRAINT FK_LocationOption_Booking FOREIGN KEY (BookingRequestId)
        REFERENCES BookingRequests(Id) ON DELETE CASCADE,
    CONSTRAINT CHK_LocationOption_Number CHECK (OptionNumber BETWEEN 1 AND 3)
);
 

ALTER TABLE BookingRequests
    ADD CONSTRAINT FK_Booking_ConfirmedLocation
    FOREIGN KEY (ConfirmedLocationOptionId) REFERENCES BookingLocationOptions(Id);

 
/* Helpful indexes */
CREATE INDEX IX_BookingRequests_Requester ON BookingRequests(RequesterId);
CREATE INDEX IX_BookingRequests_Target    ON BookingRequests(TargetId);
CREATE INDEX IX_BookingRequests_Date      ON BookingRequests(Date);
CREATE INDEX IX_ConnectionRequests_Target ON ConnectionRequests(TargetId);