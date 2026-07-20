using Microsoft.EntityFrameworkCore;
using NedAcademyWebsite.Models;

namespace NedAcademyWebsite.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        // 1. Seed System Settings
        if (!context.SystemSettings.Any())
        {
            context.SystemSettings.Add(new SystemSetting
            {
                UniversityName = "NED Academy",
                Slogan = "Pioneering the Next Frontier of Learning & Technology",
                LogoPath = "/uploads/logo.png",
                LogoDarkPath = "/uploads/logo_dark.png",
                FaviconPath = "/uploads/favicon.ico",
                ContactEmail = "info@nedacademy.edu",
                ContactPhone = "+92-21-99261261",
                Address = "NED Academy, NED University of Engineering & Technology, Main University Road, Karachi - 75270, Pakistan.",
                MapsLatitude = 24.9317,
                MapsLongitude = 67.1136,
                SocialFacebook = "https://facebook.com/nedacademy",
                SocialX = "https://x.com/nedacademy",
                SocialLinkedIn = "https://linkedin.com/company/nedacademy",
                SocialYouTube = "https://youtube.com/nedacademy",
                HeroTitle = "Pioneering the Next Frontier of Learning & Technology.",
                HeroSubtitle = "NED Academy represents excellence, industry-integrated paths, and cutting-edge research. We train creators, architects, and visionaries to lead global technical revolutions.",
                HeroVideoPath = "",
                HeroImagePath = "/uploads/hero_widget.png",
                AboutManifesto = "NED Academy was established to bridge the gap between abstract academic theories and modern engineering outputs. Grounded in the prestigious history of technical competence, we deliver accelerated programs to prepare architects of tomorrow.",
                AboutMission = "To enable future pioneers with project-first knowledge, fostering robust engineering models, high-performance computing competencies, and strategic industrial integration.",
                AboutVision = "To become a premier global hub of innovation, recognized for raising engineering standards, supporting open-source development, and producing creative technical leaders.",
                FooterCopyrightText = "NED Academy. All rights reserved."
            });
            context.SaveChanges();
        }

        // 2. Seed Departments
        if (!context.Departments.Any())
        {
            context.Departments.AddRange(
                new Department { Code = "CS-AI", Name = "Computer Science & AI" },
                new Department { Code = "ARCH-GOV", Name = "Architecture & Governance" },
                new Department { Code = "CIVIL-INFRA", Name = "Civil & Infrastructure" }
            );
            context.SaveChanges();
        }

        // Retrieve seeded departments for foreign keys
        var deptCs = context.Departments.FirstOrDefault(d => d.Code == "CS-AI");
        var deptArch = context.Departments.FirstOrDefault(d => d.Code == "ARCH-GOV");
        var deptCivil = context.Departments.FirstOrDefault(d => d.Code == "CIVIL-INFRA");

        // 3. Seed Faculty Members
        if (!context.FacultyMembers.Any() && deptCs != null && deptArch != null && deptCivil != null)
        {
            context.FacultyMembers.AddRange(
                new Faculty
                {
                    Name = "Prof. Dr. Muhammad Ali Ismail",
                    PhotoPath = "MI",
                    Designation = "Professor / CS Chairman",
                    Qualification = "Ph.D. in Computer Engineering (NED UET)",
                    DepartmentId = deptCs.Id,
                    Biography = "Dr. Muhammad Ali Ismail has over two decades of research experience in parallel architectures, HPC structures, and grid designs.",
                    ResearchAreas = "High-performance computing (HPC), distributed AI systems, GPU cluster designs.",
                    Email = "chairman-cs@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Dr. Danish Farooq",
                    PhotoPath = "DF",
                    Designation = "Associate Professor",
                    Qualification = "Ph.D. in Systems Security (Bonn, Germany)",
                    DepartmentId = deptCs.Id,
                    Biography = "Dr. Danish Farooq is a systems security researcher focusing on hardware interfaces security and network pen-testing.",
                    ResearchAreas = "Firmware hardening, smart systems validation pipelines, IoT pen-tests.",
                    Email = "dfarooq@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Dr. Maria Kazi",
                    PhotoPath = "MK",
                    Designation = "Assistant Professor",
                    Qualification = "Ph.D. in Deep Learning (Imperial, UK)",
                    DepartmentId = deptCs.Id,
                    Biography = "Dr. Maria Kazi works on transformer networks optimization, distributed deep learning weight pipelines, and natural language tasks.",
                    ResearchAreas = "Transformers architectures, neural processing, large language models.",
                    Email = "mkazi@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Prof. Dr. Noman Ahmed",
                    PhotoPath = "NA",
                    Designation = "Dean of Planning",
                    Qualification = "Ph.D. in Urban Governance (Loughborough, UK)",
                    DepartmentId = deptArch.Id,
                    Biography = "Dr. Noman Ahmed is a senior academic policy expert, researching public governance and sustainable civic systems.",
                    ResearchAreas = "Educational ethics, sustainable infrastructure models, planning boards.",
                    Email = "dean@nedacademy.edu"
                },
                new Faculty
                {
                    Name = "Prof. Dr. Sarosh H. Lodi",
                    PhotoPath = "SL",
                    Designation = "Professor / Advisor Lead",
                    Qualification = "Ph.D. in structural mechanics (UC Irvine, USA)",
                    DepartmentId = deptCivil.Id,
                    Biography = "Dr. Sarosh H. Lodi has led infrastructure design grids and structural diagnostics research globally.",
                    ResearchAreas = "Infrastructure modeling, smart grid load analysis.",
                    Email = "slodi@nedacademy.edu"
                }
            );
            context.SaveChanges();
        }

        // 4. Seed Leadership Registry
        if (!context.LeadershipRegistry.Any())
        {
            context.LeadershipRegistry.AddRange(
                new Leadership
                {
                    Name = "Hafiz Muhammad Saad",
                    Designation = "Founder & Patron-in-Chief",
                    Qualification = "B.E. in Software Engineering & Creative Systems (NED)",
                    Biography = "Technology represents the ultimate vehicle for human scalability and impact. NED Academy is our pledge to bypass structural lags in technical education, training next-generation systems engineers to build tomorrow's computational frameworks.",
                    PhotoPath = "HMS",
                    Email = "founder@nedacademy.edu",
                    Rank = 1
                },
                new Leadership
                {
                    Name = "Prof. Dr. Sarosh H. Lodi",
                    Designation = "Chancellor Advisor",
                    Qualification = "Ph.D. in Structural Engineering (UC Irvine, USA)",
                    Biography = "Coordinates the general academic steering board and guides smart infrastructure syllabus tracks.",
                    PhotoPath = "SL",
                    Email = "director@nedacademy.edu",
                    Rank = 2
                },
                new Leadership
                {
                    Name = "Prof. Dr. Noman Ahmed",
                    Designation = "Dean of Planning Board",
                    Qualification = "Ph.D. in Governance (Loughborough, UK)",
                    Biography = "Directs strategic educational guidelines and verifies ethics constraints inside syllabus frameworks.",
                    PhotoPath = "NA",
                    Email = "dean@nedacademy.edu",
                    Rank = 3
                },
                new Leadership
                {
                    Name = "Prof. Dr. Muhammad Ali Ismail",
                    Designation = "Chairman of Computing",
                    Qualification = "Ph.D. in Computer Engineering (NED UET)",
                    Biography = "Supervises computer engineering modules and handles high-performance computing lab allocations.",
                    PhotoPath = "MI",
                    Email = "chairman-cs@nedacademy.edu",
                    Rank = 4
                }
            );
            context.SaveChanges();
        }

        // 5. Seed Degree Programs
        if (!context.DegreePrograms.Any() && deptCs != null)
        {
            context.DegreePrograms.AddRange(
                new DegreeProgram
                {
                    Name = "BS Software Engineering (Accelerated & Systems)",
                    Code = "BS-SE",
                    DepartmentId = deptCs.Id,
                    Duration = "4 Years (8 Semesters)",
                    Eligibility = "Higher Secondary School Certificate (Pre-Engineering) with minimum 60% marks, or equivalent (A-Levels with physics, chemistry, and mathematics).",
                    Description = "Focuses on large-scale distributed systems, compiler design, cloud-native microservices, and database engine structures. Rather than learning just coding, students learn how to design architectures that manage billions of requests securely.",
                    LearningOutcomes = "Design resilient microservices using containerization and serverless patterns\nDevelop highly optimized compiler passes and custom low-overhead database systems\nDeploy secure automation pipelines with automated unit testing integration",
                    CareerOpportunities = "Systems Engineer\nCloud Solutions Architect\nBackend Engineer\nDevSecOps Manager",
                    ThumbnailPath = "",
                    BannerPath = "",
                    BrochurePath = "Prospectus_2026.pdf"
                },
                new DegreeProgram
                {
                    Name = "BS Artificial Intelligence & High-Performance Computing",
                    Code = "BS-AI",
                    DepartmentId = deptCs.Id,
                    Duration = "4 Years (8 Semesters)",
                    Eligibility = "Higher Secondary School Certificate (Pre-Engineering / Computer Science) with minimum 60% marks, or equivalent.",
                    Description = "Combines deep learning models with compute infrastructure. Students build neural network architectures and learn how to optimize training runs across large multi-GPU clusters, design custom vector databases, and manage distributed ML pipelines.",
                    LearningOutcomes = "Design and train custom neural network architectures using PyTorch/TensorFlow\nWrite parallel programs utilizing CUDA and MPI library routines for GPU grids\nStructure scalable data pipelines with vector database optimization",
                    CareerOpportunities = "Machine Learning Architect\nHPC Operations Analyst\nData Platform Engineer\nAI Research Scientist",
                    ThumbnailPath = "",
                    BannerPath = "",
                    BrochurePath = "Prospectus_2026.pdf"
                },
                new DegreeProgram
                {
                    Name = "BS Cyber Security & Smart Systems",
                    Code = "BS-CY",
                    DepartmentId = deptCs.Id,
                    Duration = "4 Years (8 Semesters)",
                    Eligibility = "Higher Secondary School Certificate (Pre-Engineering / Computer Science) with minimum 60% marks, or equivalent.",
                    Description = "Focuses on adversarial security engineering, firmware analysis, smart grid cybersecurity, and distributed hardware security. Students learn penetration testing methodologies, cryptography, threat modeling, and secure code auditing.",
                    LearningOutcomes = "Perform comprehensive threat modeling and vulnerability scans across cloud clusters\nConduct firmware analysis and secure hardware interfaces\nImplement cryptographic key architectures and secure communication lines",
                    CareerOpportunities = "Penetration Tester\nApplication Security Architect\nCryptographic System Engineer\nSmart Systems Consultant",
                    ThumbnailPath = "",
                    BannerPath = "",
                    BrochurePath = "Prospectus_2026.pdf"
                }
            );
            context.SaveChanges();
        }

        // 6. Seed Research Items
        if (!context.ResearchItems.Any())
        {
            context.ResearchItems.AddRange(
                new ResearchItem
                {
                    Category = "Lab",
                    Title = "HPC & GPU Compute Cluster Lab",
                    Description = "Focuses on compiling passes for multi-GPU arrays, optimizing distributed transformer models, and designing custom high-throughput memory pipelines.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Lab",
                    Title = "Threat Intelligence & Pen-Testing Lab",
                    Description = "Researches firmware hardening strategies, smart grid vulnerabilities, secure key exchange mechanisms, and automated threat modeling architectures.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Lab",
                    Title = "Smart Grid & Decarbonized Infrastructure Lab",
                    Description = "Focuses on structural health simulation models, digital twins of civic resources, and decentralized energy distribution algorithms.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Project",
                    Title = "Decentralized Load Balancer (DLB)",
                    Description = "A decentralized system utility engineered to distribute routing workloads across micro-clusters dynamically. Operates with sub-millisecond latencies and low memory footprints.",
                    ImagePath = "",
                    Link = ""
                },
                new ResearchItem
                {
                    Category = "Project",
                    Title = "High-Throughput Training Pipelines",
                    Description = "Researches optimized weights training passes across local GPU arrays, compiling custom memory allocators to prevent parallel processing bottlenecks.",
                    ImagePath = "",
                    Link = ""
                }
            );
            context.SaveChanges();
        }

        // 7. Seed Campus Facilities
        if (!context.CampusFacilities.Any())
        {
            context.CampusFacilities.AddRange(
                new CampusFacility { Title = "Modern Classrooms", Description = "Equipped with high-resolution laser displays, acoustic dampening layouts, and active modular desks for group sprint activities.", IconClass = "fa-solid fa-chalkboard-user" },
                new CampusFacility { Title = "Resource Library", Description = "Features high-speed search terminals, digital journal access databases, private research cabins, and reference textbooks.", IconClass = "fa-solid fa-book-open" },
                new CampusFacility { Title = "Advanced Laboratories", Description = "Smart grid simulators, systems validation testing utilities, and computational physics testing suites.", IconClass = "fa-solid fa-flask" },
                new CampusFacility { Title = "HPC Computer Labs", Description = "Equipped with high-end workstations connected directly to local GPU servers for executing machine learning models.", IconClass = "fa-solid fa-server" },
                new CampusFacility { Title = "Innovation Center", Description = "A startup sandbox area with whiteboard walls, projector sets, and meeting booths designed for project incubation.", IconClass = "fa-solid fa-lightbulb" },
                new CampusFacility { Title = "Auditorium", Description = "A premium, 500-seat theater layout equipped with linear acoustic setups, hosting tech symposiums and orientations.", IconClass = "fa-solid fa-people-roof" },
                new CampusFacility { Title = "Sports Complex", Description = "Includes a modern indoor gym, basketball courts, and outdoor running tracks for wellness activities.", IconClass = "fa-solid fa-volleyball" },
                new CampusFacility { Title = "Cafeteria", Description = "A hygienic dining hub offering diverse cuisines, fast internet access, and comfortable collaboration layouts.", IconClass = "fa-solid fa-utensils" },
                new CampusFacility { Title = "Campus Mosque", Description = "A beautifully designed, spacious prayer area built for spiritual tranquility and daily congregations.", IconClass = "fa-solid fa-mosque" },
                new CampusFacility { Title = "Medical Center", Description = "Offers 24/7 medical response parameters, emergency first aid tools, and wellness diagnosis consults.", IconClass = "fa-solid fa-house-medical" }
            );
            context.SaveChanges();
        }

        // 8. Seed Download Files
        if (!context.DownloadFiles.Any())
        {
            context.DownloadFiles.AddRange(
                new DownloadFile { Title = "University Prospectus 2026", Category = "admissions", FilePath = "Prospectus_2026.pdf", SizeString = "PDF | 4.2 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Admission Guidelines Form", Category = "admissions", FilePath = "Admission_Form.pdf", SizeString = "PDF | 1.1 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Academic Calendar Fall 2026", Category = "academic", FilePath = "Academic_Calendar_Fall2026.pdf", SizeString = "PDF | 0.9 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Research Incubation Guidelines", Category = "policy", FilePath = "Research_Policy_Handbook.pdf", SizeString = "PDF | 2.5 MB", Extension = ".pdf" },
                new DownloadFile { Title = "Student Honor Code & Ethics", Category = "policy", FilePath = "Student_Honor_Code.pdf", SizeString = "PDF | 1.5 MB", Extension = ".pdf" }
            );
            context.SaveChanges();
        }

        // 9. Seed FAQ Items
        if (!context.FAQItems.Any())
        {
            context.FAQItems.AddRange(
                new FAQItem { Category = "General", Question = "How long is the admission processing cycle?", Answer = "The admissions processing cycle generally takes 3 to 4 weeks. Once applications are filed on the online portal, aptitude assessments are conducted, followed by panel interviews for qualifying applicants. Final placement letters are dispatched within 5 days of interviews." },
                new FAQItem { Category = "Academics", Question = "What programming languages are taught in BS Software Engineering?", Answer = "Our curriculum focuses on concepts rather than specific syntaxes. However, students build systems using Rust, C++, C, Go, and Python. We prioritize low-level memory control, network protocol programming, and cloud concurrency." },
                new FAQItem { Category = "General", Question = "Does NED Academy provide credits transfer policies?", Answer = "Yes. Credit transfer applications are reviewed on a case-by-case basis by the Academic Directors Board. Courses completed at HEC-recognized national universities or foreign institutions with a grade of B or above are eligible for review." },
                new FAQItem { Category = "Technical", Question = "Are there separate computing grids for undergraduate research?", Answer = "Yes. Senior-year student groups and research assistants are assigned dedicated compute slices on our high-performance cluster. GPU allocation requests are authorized through dynamic lab supervisors." },
                new FAQItem { Category = "Academics", Question = "What is the student-faculty ratio in computer science streams?", Answer = "We maintain a strict 12:1 student-to-faculty ratio across all computational engineering classes. This ensures that every learner receives individual code reviews and architecture design feedback." }
            );
            context.SaveChanges();
        }

        // 10. Seed News Items
        if (!context.NewsItems.Any())
        {
            context.NewsItems.AddRange(
                new NewsItem
                {
                    Title = "Decentralized Smart Grid Optimizer Launched",
                    Category = "Research",
                    Content = "Our smart systems engineering cohort, under Prof. Dr. Sarosh H. Lodi, has successfully validated prototypes of a decentralized load optimizer. The research will be published in upcoming global engineering journals.",
                    PublishDate = DateTime.Now.AddDays(-1),
                    Status = "Published",
                    FeaturedImagePath = ""
                },
                new NewsItem
                {
                    Title = "CS Department Deploys GPU Training Cluster",
                    Category = "General",
                    Content = "Under Dr. Muhammad Ali Ismail's oversight, a local multi-GPU cluster has been wired up to support student deep learning tasks. The cluster offers parallel acceleration capacities for weights training.",
                    PublishDate = DateTime.Now.AddDays(-5),
                    Status = "Published",
                    FeaturedImagePath = ""
                },
                new NewsItem
                {
                    Title = "Incubation Agreement Finalized with Cloud Partners",
                    Category = "Partnership",
                    Content = "NED Academy has locked in a strategic alliance to provide senior-year students with cloud compute credits and mentorship lines with global solution architects.",
                    PublishDate = DateTime.Now.AddDays(-12),
                    Status = "Published",
                    FeaturedImagePath = ""
                }
            );
            context.SaveChanges();
        }

        // 11. Seed Event Items
        if (!context.EventItems.Any())
        {
            context.EventItems.AddRange(
                new EventItem { Title = "AI & Cloud Architecture Symposium", Date = DateTime.Today.AddDays(25), TimeString = "10:00 AM - 04:00 PM", Venue = "Main Auditorium", BannerImagePath = "" },
                new EventItem { Title = "Fall Semester Orientation 2026", Date = DateTime.Today.AddDays(42), TimeString = "09:30 AM - 12:30 PM", Venue = "Main Campus Plaza", BannerImagePath = "" },
                new EventItem { Title = "Annual Tech & Research Expo", Date = DateTime.Today.AddDays(64), TimeString = "09:00 AM - 05:00 PM", Venue = "CS Innovation Wing", BannerImagePath = "" }
            );
            context.SaveChanges();
        }
    }
}
