-- ===================================================================
-- Учебная практика УП.01.01
-- Студент: Фараджов Руслан Эльмар оглы, группа 4ИСИП-523
-- Преподаватель: Аксёнова Т.Г.
-- Тема: Проектирование и разработка базы данных для сервиса
--       по автоматизированному составлению расписания (учебного, рабочего)
-- База данных: DB_Faradzhov_Schedule (MS SQL Server, 3НФ)
-- ===================================================================

USE master;
GO

IF DB_ID('DB_Faradzhov_Schedule') IS NOT NULL
BEGIN
    ALTER DATABASE DB_Faradzhov_Schedule SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DB_Faradzhov_Schedule;
END
GO

CREATE DATABASE DB_Faradzhov_Schedule COLLATE Cyrillic_General_CI_AS;
GO

USE DB_Faradzhov_Schedule;
GO

-- 1. Таблица учетных записей пользователей
CREATE TABLE [user_account] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [login] nvarchar(100) NOT NULL UNIQUE,
    [password_hash] nvarchar(255) NOT NULL,
    [role] nvarchar(50) NOT NULL,
    [full_name] nvarchar(255) NOT NULL,
    [email] nvarchar(255) NULL,
    [phone] nvarchar(50) NULL,
    [photo] nvarchar(max) NULL
);
GO

-- 2. Таблица отделений / кафедр
CREATE TABLE [department] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(255) NOT NULL UNIQUE,
    [code] nvarchar(50) NOT NULL,
    [head_name] nvarchar(255) NULL
);
GO

-- 3. Таблица преподавателей и сотрудников
CREATE TABLE [teacher] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [user_id] int NULL,
    [department_id] int NOT NULL,
    [full_name] nvarchar(255) NOT NULL,
    [academic_degree] nvarchar(150) NOT NULL,
    [email] nvarchar(255) NULL,
    [phone] nvarchar(50) NULL,
    [photo] nvarchar(max) NULL,
    CONSTRAINT FK_Teacher_User FOREIGN KEY ([user_id]) REFERENCES [user_account] ([id]) ON DELETE SET NULL,
    CONSTRAINT FK_Teacher_Department FOREIGN KEY ([department_id]) REFERENCES [department] ([id]) ON DELETE CASCADE
);
GO

-- 4. Таблица учебных аудиторий и кабинетов
CREATE TABLE [classroom] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [room_number] nvarchar(50) NOT NULL UNIQUE,
    [building] nvarchar(100) NOT NULL,
    [capacity] int NOT NULL,
    [room_type] nvarchar(100) NOT NULL,
    [has_projector] bit NOT NULL DEFAULT 1
);
GO

-- 5. Таблица учебных групп
CREATE TABLE [study_group] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [group_name] nvarchar(100) NOT NULL UNIQUE,
    [department_id] int NOT NULL,
    [course_number] int NOT NULL,
    [student_count] int NOT NULL,
    CONSTRAINT FK_Group_Department FOREIGN KEY ([department_id]) REFERENCES [department] ([id]) ON DELETE CASCADE
);
GO

-- 6. Таблица дисциплин и предметов
CREATE TABLE [subject] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(255) NOT NULL UNIQUE,
    [total_hours] int NOT NULL,
    [department_id] int NOT NULL,
    CONSTRAINT FK_Subject_Department FOREIGN KEY ([department_id]) REFERENCES [department] ([id]) ON DELETE CASCADE
);
GO

-- 7. Таблица временных слотов (звонков / расписания пар)
CREATE TABLE [time_slot] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [pair_number] int NOT NULL,
    [start_time] nvarchar(10) NOT NULL,
    [end_time] nvarchar(10) NOT NULL
);
GO

-- 8. Таблица элементов расписания (занятий)
CREATE TABLE [schedule_item] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [day_of_week] nvarchar(50) NOT NULL,
    [time_slot_id] int NOT NULL,
    [group_id] int NOT NULL,
    [subject_id] int NOT NULL,
    [teacher_id] int NOT NULL,
    [classroom_id] int NOT NULL,
    [lesson_type] nvarchar(50) NOT NULL,
    [week_type] nvarchar(50) NOT NULL DEFAULT N'Все недели',
    [status] nvarchar(50) NOT NULL DEFAULT N'Утверждено',
    [created_at] datetime NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Schedule_Slot FOREIGN KEY ([time_slot_id]) REFERENCES [time_slot] ([id]),
    CONSTRAINT FK_Schedule_Group FOREIGN KEY ([group_id]) REFERENCES [study_group] ([id]),
    CONSTRAINT FK_Schedule_Subject FOREIGN KEY ([subject_id]) REFERENCES [subject] ([id]),
    CONSTRAINT FK_Schedule_Teacher FOREIGN KEY ([teacher_id]) REFERENCES [teacher] ([id]),
    CONSTRAINT FK_Schedule_Room FOREIGN KEY ([classroom_id]) REFERENCES [classroom] ([id])
);
GO

-- ===================================================================
-- НАЧАЛЬНОЕ НАПОЛНЕНИЕ ДАННЫМИ
-- ===================================================================

-- 1. Пользователи
-- Пароль admin123 -> SHA-1: F865B53623B121FD34EE5426C792E5C33AF8C227
-- Пароль user123  -> SHA-1: 38762CF7F55934B34D179AE6A4C80CADCCBB7F0A
INSERT INTO [user_account] ([login], [password_hash], [role], [full_name], [email], [phone], [photo])
VALUES
(N'admin', N'F865B53623B121FD34EE5426C792E5C33AF8C227', N'Admin', N'Фараджов Руслан Эльмар оглы', N'faradzhov@edu.fa.ru', N'+7 (999) 777-11-22', N'/res/admin.png'),
(N'aksenova', N'F865B53623B121FD34EE5426C792E5C33AF8C227', N'Admin', N'Аксёнова Татьяна Геннадьевна', N'aksenova@fa.ru', N'+7 (999) 111-22-33', N'/res/user1.png'),
(N'ruslan', N'38762CF7F55934B34D179AE6A4C80CADCCBB7F0A', N'User', N'Фараджов Руслан', N'ruslan.f@fa.ru', N'+7 (999) 222-33-44', N'/res/user2.png'),
(N'ivanov', N'38762CF7F55934B34D179AE6A4C80CADCCBB7F0A', N'User', N'Иванов Иван Иванович', N'ivanov@fa.ru', N'+7 (999) 333-44-55', NULL),
(N'smirnova', N'38762CF7F55934B34D179AE6A4C80CADCCBB7F0A', N'User', N'Смирнова Анна Павловна', N'smirnova@fa.ru', N'+7 (999) 444-55-66', NULL);
GO

-- 2. Отделения / Направления
INSERT INTO [department] ([name], [code], [head_name])
VALUES
(N'Информационные системы и программирование', N'ИСиП', N'Кузнецова Ольга Викторовна'),
(N'Обеспечение информационной безопасности автоматизированных систем', N'ОИБАС', N'Григорьев Сергей Николаевич');
GO

-- 3. Преподаватели
INSERT INTO [teacher] ([user_id], [department_id], [full_name], [academic_degree], [email], [phone], [photo])
VALUES
(2, 1, N'Аксёнова Татьяна Геннадьевна', N'Кандидат пед. наук, ВКК', N'aksenova@fa.ru', N'+7 (999) 111-22-33', N'/res/user1.png'),
(NULL, 1, N'Гордов Денис Александрович', N'Старший преподаватель, магистр ВТ', N'gordov@fa.ru', N'+7 (999) 111-22-44', NULL),
(NULL, 1, N'Соколов Алексей Игоревич', N'Преподаватель высшей категории', N'sokolov@fa.ru', N'+7 (999) 111-22-55', NULL),
(NULL, 2, N'Морозова Елена Васильевна', N'Доцент кафедры информационной безопасности', N'morozova@fa.ru', N'+7 (999) 111-22-66', NULL),
(NULL, 2, N'Васильев Дмитрий Сергеевич', N'Преподаватель технической защиты информации', N'vasiliev@fa.ru', N'+7 (999) 111-22-77', NULL);
GO

-- 4. Аудитории
INSERT INTO [classroom] ([room_number], [building], [capacity], [room_type], [has_projector])
VALUES
(N'301-ИТ', N'Главный корпус', 30, N'Компьютерный класс', 1),
(N'305-Лек', N'Главный корпус', 90, N'Лекционная аудитория', 1),
(N'412-Лаб', N'Лабораторный корпус', 26, N'Лаборатория сетей', 1),
(N'204-Сем', N'Главный корпус', 35, N'Семинарская аудитория', 1),
(N'508-ИТ', N'Лабораторный корпус', 28, N'Компьютерный класс', 1),
(N'102-Конф', N'Главный корпус', 120, N'Конференц-зал', 1);
GO

-- 5. Учебные группы
INSERT INTO [study_group] ([group_name], [department_id], [course_number], [student_count])
VALUES
(N'4ИСИП-523', 1, 4, 28),
(N'3ИСИП-423', 1, 3, 26),
(N'2ИСИП-324', 1, 2, 30),
(N'4ОИБАС-523', 2, 4, 25),
(N'3ОИБАС-423', 2, 3, 24);
GO

-- 6. Дисциплины
INSERT INTO [subject] ([name], [total_hours], [department_id])
VALUES
(N'Разработка программных модулей (ПМ.01)', 144, 1),
(N'Поддержка и тестирование программных модулей (МДК.01.02)', 108, 1),
(N'Технология разработки и защиты баз данных', 120, 1),
(N'Проектирование и дизайн информационных систем', 96, 1),
(N'Информационная безопасность и криптографическая защита', 108, 2),
(N'Безопасность компьютерных сетей и систем', 96, 2);
GO

-- 7. Временные слоты (звонки пар)
INSERT INTO [time_slot] ([pair_number], [start_time], [end_time])
VALUES
(1, N'08:30', N'10:00'),
(2, N'10:15', N'11:45'),
(3, N'12:15', N'13:45'),
(4, N'14:00', N'15:30'),
(5, N'15:45', N'17:15'),
(6, N'17:30', N'19:00');
GO

-- 8. Элементы расписания занятий
INSERT INTO [schedule_item] ([day_of_week], [time_slot_id], [group_id], [subject_id], [teacher_id], [classroom_id], [lesson_type], [week_type], [status])
VALUES
-- Понедельник (4ИСИП-523)
(N'Понедельник', 1, 1, 1, 1, 1, N'Лабораторная работа', N'Все недели', N'Утверждено'),
(N'Понедельник', 2, 1, 2, 1, 1, N'Практическое занятие', N'Все недели', N'Утверждено'),
(N'Понедельник', 3, 1, 3, 3, 2, N'Лекция', N'Все недели', N'Утверждено'),

-- Вторник (4ИСИП-523)
(N'Вторник', 2, 1, 4, 2, 5, N'Лабораторная работа', N'Числитель', N'Утверждено'),
(N'Вторник', 3, 1, 1, 1, 1, N'Практическое занятие', N'Все недели', N'Утверждено'),
(N'Вторник', 4, 1, 3, 3, 4, N'Семинар', N'Все недели', N'Утверждено'),

-- Среда (4ИСИП-523)
(N'Среда', 1, 1, 2, 1, 1, N'Лабораторная работа', N'Все недели', N'Утверждено'),
(N'Среда', 2, 1, 4, 2, 5, N'Практическое занятие', N'Знаменатель', N'Утверждено'),
(N'Среда', 3, 1, 5, 4, 3, N'Лабораторная работа', N'Все недели', N'Утверждено'),

-- Четверг (4ИСИП-523)
(N'Четверг', 2, 1, 3, 3, 2, N'Лекция', N'Все недели', N'Утверждено'),
(N'Четверг', 3, 1, 2, 1, 1, N'Консультация', N'Все недели', N'Утверждено'),

-- Пятница (4ИСИП-523)
(N'Пятница', 1, 1, 1, 1, 1, N'Лабораторная работа', N'Все недели', N'Утверждено'),
(N'Пятница', 2, 1, 4, 2, 5, N'Лабораторная работа', N'Все недели', N'Утверждено'),

-- Занятия для группы 3ИСИП-423
(N'Понедельник', 2, 2, 3, 3, 2, N'Лекция', N'Все недели', N'Утверждено'),
(N'Понедельник', 3, 2, 4, 2, 5, N'Практическое занятие', N'Все недели', N'Утверждено'),
(N'Вторник', 1, 2, 1, 1, 1, N'Лабораторная работа', N'Все недели', N'Утверждено'),
(N'Вторник', 2, 2, 2, 1, 1, N'Практическое занятие', N'Все недели', N'Утверждено'),
(N'Среда', 2, 2, 3, 3, 4, N'Семинар', N'Все недели', N'Утверждено'),

-- Занятия для группы 4ОИБАС-523
(N'Понедельник', 4, 4, 5, 4, 3, N'Лабораторная работа', N'Все недели', N'Утверждено'),
(N'Вторник', 3, 4, 5, 4, 3, N'Практическое занятие', N'Все недели', N'Утверждено'),
(N'Среда', 1, 4, 6, 5, 4, N'Лекция', N'Все недели', N'Утверждено'),

-- Занятия для группы 3ОИБАС-423
(N'Четверг', 1, 5, 6, 5, 4, N'Лабораторная работа', N'Все недели', N'Утверждено'),
(N'Пятница', 3, 5, 6, 5, 4, N'Практическое занятие', N'Все недели', N'Утверждено');
GO
