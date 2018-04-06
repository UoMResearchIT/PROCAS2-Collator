SET IDENTITY_INSERT [dbo].[AddressTypes] ON
INSERT INTO [dbo].[AddressTypes] ([Id], [Name]) VALUES (1, N'HOME')
INSERT INTO [dbo].[AddressTypes] ([Id], [Name]) VALUES (2, N'GP')
SET IDENTITY_INSERT [dbo].[AddressTypes] OFF

SET IDENTITY_INSERT [dbo].[AppUsers] ON
INSERT INTO [dbo].[AppUsers] ([Id], [UserCode], [Active], [SuperUser], [SystemUser]) VALUES (1, N'andrew.jerrison@manchester.ac.uk', 1, 1, 0)
INSERT INTO [dbo].[AppUsers] ([Id], [UserCode], [Active], [SuperUser], [SystemUser]) VALUES (2, N'CRA_Automatic', 1, 0, 1)
SET IDENTITY_INSERT [dbo].[AppUsers] OFF

INSERT INTO [dbo].[AspNetRoles] ([Id], [Name]) VALUES (N'60c25c47-f649-4450-b043-e43312058857', N'General')
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name]) VALUES (N'd8474941-7139-4315-979e-30e42605635e', N'Super')

SET IDENTITY_INSERT [dbo].[EventTypes] ON
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (1, N'Participant Created', N'001')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (2, N'Consent Received', N'002')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (3, N'Full Details Added', N'003')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (4, N'Field Updated', N'004')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (5, N'Deleted', N'005')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (6, N'Risk Letter Asked For', N'006')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (7, N'Risk Letter Received', N'007')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (8, N'Histology header created', N'008')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (9, N'Histology focus created', N'009')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (10, N'Histology header deleted', N'010')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (11, N'Histology focus deleted', N'011')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (12, N'Screening Outcomes uploaded', N'012')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (13, N'Histology focus updated', N'013')
INSERT INTO [dbo].[EventTypes] ([Id], [Name], [Code]) VALUES (14, N'Histology header updated', N'014')
SET IDENTITY_INSERT [dbo].[EventTypes] OFF

SET IDENTITY_INSERT [dbo].[HistologyLookups] ON
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (1, N'SIDE', N'Left')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (2, N'SIDE', N'Right')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (3, N'SIDE', N'Bilateral')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (4, N'TYPE', N'Screen')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (5, N'TYPE', N'Interval')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (6, N'SIDE', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (7, N'TYPE', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (8, N'DCIS', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (9, N'DCIS', N'Low')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (10, N'DCIS', N'High')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (11, N'DCIS', N'Intermediate')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (12, N'DCIS', N'Intermediate + High')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (13, N'INVASIVE', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (14, N'INVASIVE', N'Both')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (15, N'INVASIVE', N'CIS')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (16, N'INVASIVE', N'Invasive')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (17, N'PATH', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (18, N'PATH', N'IDC with DCIS')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (19, N'PATH', N'IDC - Lobular (alveolar variant)')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (20, N'PATH', N'Invasive lobular carcinoma')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (21, N'PATH', N'IDC + DCIS - cribriform')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (22, N'VASCULAR', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (23, N'VASCULAR', N'No')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (24, N'VASCULAR', N'Yes')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (25, N'VASCULAR', N'Probable')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (26, N'HER2', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (27, N'HER2', N'0')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (28, N'HER2', N'1')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (29, N'HER2', N'2')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (30, N'HER2', N'2 Fish Amplified')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (31, N'HER2', N'2 Fish Not Amplified')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (32, N'HER2', N'3')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (33, N'TNM', N' Unknown')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (34, N'DCIS', N'Low + High')
INSERT INTO [dbo].[HistologyLookups] ([Id], [LookupType], [LookupDescription]) VALUES (35, N'DCIS', N'Low + Intermediate')
SET IDENTITY_INSERT [dbo].[HistologyLookups] OFF

SET IDENTITY_INSERT [dbo].[ParticipantLookups] ON
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (1, N'CHEMO', N'Not appropriate', N'CHEMONA')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (2, N'CHEMO', N'Appropriate but prescription not filled', N'CHEMOAPPNOT')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (3, N'CHEMO', N'Appropriate and prescription filled', N'CHEMOAPPFILL')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (4, N'INITIAL', N'1. Technical recall', N'INI_TECH')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (5, N'INITIAL', N'2. Recall for assessment', N'INI_ASSESS')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (6, N'INITIAL', N'3. Routine recall', N'INI_ROUTINE')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (7, N'TECH', N'Routine recall', N'TECH_ROUTINE')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (8, N'TECH', N'DNA appointment', N'TECH_DNA')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (9, N'TECH', N'Recall for assessment - routine recall', N'TECH_REC_ROUTINE')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (10, N'TECH', N'Recall for assessment - breast cancer', N'TECH_REC_CANCER')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (11, N'TECH', N'Recall for assessment - enhanced surveillance', N'TECH_REC_SURV')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (12, N'RECALL', N'Routine recall', N'RECALL_ROUTINE')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (13, N'RECALL', N'DNA appointment', N'RECALL_DNA')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (14, N'RECALL', N'Breast cancer', N'RECALL_CANCER')
INSERT INTO [dbo].[ParticipantLookups] ([Id], [LookupType], [LookupDescription], [LookupCode]) VALUES (15, N'RECALL', N'Enhanced survelliance', N'RECALL_SURV')
SET IDENTITY_INSERT [dbo].[ParticipantLookups] OFF

SET IDENTITY_INSERT [dbo].[Questions] ON
INSERT INTO [dbo].[Questions] ([Id], [Code], [Text]) VALUES (1, N'surveyQuestion1', N'Is this question 1?')
INSERT INTO [dbo].[Questions] ([Id], [Code], [Text]) VALUES (2, N'surveyQuestion2', N'If this is question 2, what is question 3?')
INSERT INTO [dbo].[Questions] ([Id], [Code], [Text]) VALUES (3, N'surveyQuestion3', N'Have you got the answer yet?')
SET IDENTITY_INSERT [dbo].[Questions] OFF

SET IDENTITY_INSERT [dbo].[ScreeningSites] ON
INSERT INTO [dbo].[ScreeningSites] ([Id], [Name], [Code], [AddressLine1], [AddressLine2], [AddressLine3], [AddressLine4], [PostCode], [LetterFrom], [Telephone], [LogoFooterLeft], [LogoFooterRight], [LogoHeaderRight], [LogoHeaderRightHeight], [LogoHeaderRightWidth], [LogoFooterLeftWidth], [LogoFooterRightWidth], [LogoFooterLeftHeight], [LogoFooterRightHeight], [Signature], [TrustCode]) VALUES (2, N'Wythenshawe', N'WYTH', N'Nightingale Centre', N'Wythenshawe Hospital', N'Southmoor Road', N'Manchester', N'M23 9LT', N'Prof. D. Gareth Evans', N'+44 (0)161 291 4409', N'MFT_INCORPORATING', N'MFT_DISABILITY', N'MFT_LOGO', N'1.71', N'5.5', N'13', N'2.34', N'1.75', N'2.34', N'trg', N'MFT')
INSERT INTO [dbo].[ScreeningSites] ([Id], [Name], [Code], [AddressLine1], [AddressLine2], [AddressLine3], [AddressLine4], [PostCode], [LetterFrom], [Telephone], [LogoFooterLeft], [LogoFooterRight], [LogoHeaderRight], [LogoHeaderRightHeight], [LogoHeaderRightWidth], [LogoFooterLeftWidth], [LogoFooterRightWidth], [LogoFooterLeftHeight], [LogoFooterRightHeight], [Signature], [TrustCode]) VALUES (3, N'Macclesfield', N'MACC', N'Breast Screening, New Alderley House', N'Macclesfield District General Hospital', N'Victoria Road', N'Macclesfield', N'SK10 3BL', N'Prof D. Gareth Evans', N'01625 661157', NULL, NULL, N'EAST_CHESHIRE_LOGO', N'2.47', N'5.5', NULL, NULL, NULL, NULL, NULL, N'EASTCHESHIRE')
INSERT INTO [dbo].[ScreeningSites] ([Id], [Name], [Code], [AddressLine1], [AddressLine2], [AddressLine3], [AddressLine4], [PostCode], [LetterFrom], [Telephone], [LogoFooterLeft], [LogoFooterRight], [LogoHeaderRight], [LogoHeaderRightHeight], [LogoHeaderRightWidth], [LogoFooterLeftWidth], [LogoFooterRightWidth], [LogoFooterLeftHeight], [LogoFooterRightHeight], [Signature], [TrustCode]) VALUES (5, N'Lancashire', N'LANCS', N'East Lancashire Breast Screening Service', N'Area 3, Burnley General Hospital', N'Casterton Avenue', N'Burnley', N'BB10 2PQ', N'Prof. D. Gareth Evans', N'01282805301', N'EAST_LANCS_SPE', NULL, N'EAST_LANCS_LOGO', N'1.64', N'5.5', N'11', NULL, N'1.16', NULL, NULL, N'EASTLANCS')
SET IDENTITY_INSERT [dbo].[ScreeningSites] OFF



