# TrainAR

[![GitHub license](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/jblattgerste/TrainAR/blob/master/LICENSE) [![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://github.com/jblattgerste/TrainAR/graphs/commit-activity) [![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com) [![Documentation](https://github.com/jblattgerste/TrainAR/actions/workflows/documentation.yml/badge.svg)](https://jblattgerste.github.io/TrainAR/)

TrainAR is a holistic threefold combination of an interaction concept, didactic framework and authoring tool for Augmented Reality (AR) trainings on handheld Android and iOS devices ([Blattgerste et al. 2021](https://scholar.google.de/citations?view_op=view_citation&hl=en&user=k2xymcIAAAAJ&citation_for_view=k2xymcIAAAAJ:Y0pCki6q_DkC)). It is completely open source, free and offers non-programmers and programmers without AR-specific expertise aiding in the creation of interactive, engaging, procedural Augmented Reality trainings. This repository contains the technical components of the interaction concept and authoring tool of TrainAR in form of a custom Unity 2022.1. Editor Extension. It can be used with the Unity Windows, macOS (Silicon), macOS (Intel), or Linux Editor and deploy to Android and iOS devices. The authoring tool already offers features like the onboarding animations, tracking solutions, assembly placements, evaluated interaction concepts, layered feedback modalities and training assessments of TrainAR out of the box. This allows authors of AR trainings to focus on the content of the training instead of technical challenges. Authors can simply import 3D models into the tool, convert them to TrainAR objects and reference them in a visual-scripting stateflow (that is inspired by work-process-analyses) to create a procedural flow of instructions, user actions and feedback.

![Example TrainAR Authoring on a laptop with deployment to an android phone](https://raw.githubusercontent.com/jblattgerste/TrainAR/64a9372b05bf0e3656f8e2d8365d9b566a2de37d/Documentation/resources/ExampleAuthoring.png)

The idea behind TrainAR is simple: Realistic deployments of head-mounted AR devices still remain a challenge today because of high costs, missing relevant training, and novelty of interactions that require in-depth onboarding. In contrast, smartphone-based AR would be realistically scalable today, while still retaining many of the learning benefits. At least in theory. While possible, most mobile AR learning applications focus on visualisation instead of interactions today, severely limiting their application scope. In line with recent findings that, in terms of training outcome, tangible interactions are not significantly increasing retention or transfer of knowledge compared to purely virtual interaction approaches ([Knierim et al. 2020](https://scholar.google.de/citations?view_op=view_citation&hl=en&user=oHubmTIAAAAJ&cstart=20&pagesize=80&citation_for_view=oHubmTIAAAAJ:RYcK_YlVTxYC)), the idea of TrainAR is a holistic and scalable solution for procedural task training using Augmented Reality on handheld AR devices. Hereby, the idea is not to replace practical trainings but use TrainAR scenarios for concept and procedure understanding in preparation for or retention training after the practical training sessions. In line with [Gagne 1984](https://psycnet.apa.org/record/1985-05816-001), it is envisioned as a novel type of multimedia source to train _intellectual skills_ and _cognitive strategies_ but does not train associated _motor skills_.

## TrainAR Training Scenarios
Several TrainAR trainings were already developed by us, researchers from partner universities, and students using preliminary versions of the TrainAR authoring tool. They span across the contexts of medical education, nursing education, chemical engineering, science educational, manual assembly, and everyday work tasks. A list of publications for some of the trainings can be found below.

![Example TrainAR scenarios](https://raw.githubusercontent.com/jblattgerste/TrainAR/64a9372b05bf0e3656f8e2d8365d9b566a2de37d/Documentation/resources/ExampleScenarios.png)

## Documentation & Getting Started
If you want to try out already developed TrainAR trainings, here is a list of available Apps that utilize TrainAR:
- Training scenario "Preparation of a tocolytic injection" in the Heb@AR App [[Youtube](https://www.youtube.com/watch?v=CUyuzIkvvuk), [Android](https://play.google.com/store/apps/details?id=de.Mixality.HebAR) & [iOS](https://apps.apple.com/app/heb-ar/id1621822317)]
- Training scenario "Pelvis Termini" in the Heb@AR App [[Youtube](https://www.youtube.com/watch?v=arTJ3lrHRkw), [Android](https://play.google.com/store/apps/details?id=de.Mixality.HebAR) & [iOS](https://apps.apple.com/app/heb-ar/id1621822317)]

If you want to get started with creating, deploying and playing with your own TrainAR training, check out our [Getting Started Guide](https://jblattgerste.github.io/TrainAR/manual/GettingStarted.html). You can either use the example scenario that ships with this repository, or create a very simple example scene from the guide in less than half an hour. Beyond the Getting Started Guide, a full documentation is available detailing the available [visual scripting nodes](https://jblattgerste.github.io/TrainAR/manual/TrainARNodes.html), how to [create TrainAR objects](https://jblattgerste.github.io/TrainAR/manual/TrainArObjects.html) and how to [use nodes to implement action flows](https://jblattgerste.github.io/TrainAR/manual/VisualScripting.html). Additionally, a complete documentation of all [API references](https://jblattgerste.github.io/TrainAR/api/Interaction.html) is included.

A key idea behind TrainAR is the retention of the full Unity capabilities while offering a higher-level abstraction for non-programmers to get started with AR authoring. Therefore, it can also be utilized without the visual scripting and without the TrainAR authoring overlay, using the standard Unity Editor Overlay and C# programming. This could for example be interesting, if you want to implement non-procedural educational games or you are only interested in the components of the interaction concept and not so much the authoring tool itself. For this, check out the [Advanced Options](https://jblattgerste.github.io/TrainAR/manual/NoVisualScripting.html) documentation.

## Contributing to this Project
TrainAR is envisioned as a participatory project, continously improving and expanding it's quality and scope. Feel free to contribute to its source code, documentation or conceptual/didactic ideas through [Issues](https://github.com/jblattgerste/TrainAR/issues) or [Discussions](https://github.com/jblattgerste/TrainAR/discussions). You are using TrainAR for an Augmented Reality training or learning game? Show us what you created! You are using TrainAR to explore Augmented Reality trainings in a new context as your scientific research? Feel free to add your publications to our list of publications below so others can use it as a reference.

## Publication List
#### TrainAR Framework:
- Blattgerste, J.; Behrends, J.; Pfeiffer, T. (2023) [TrainAR: An Open-Source Visual Scripting-Based Authoring Tool for Procedural Mobile Augmented Reality Trainings](https://www.mdpi.com/2078-2489/14/4/219). Information, 14 (4), 219.
- Blattgerste, J., & Pfeiffer, T. (2022). [TrainAR: Ein Augmented Reality Training Autorensystem](https://dl.gi.de/bitstream/handle/20.500.12116/39383/AVRiL2022_Proceedings_06_3182.pdf?sequence=1&isAllowed=y). Wettbewerbsband AVRiL 2022.
- Blattgerste, J., Luksch, K., Lewa, C., & Pfeiffer, T. (2021). [TrainAR: A Scalable Interaction Concept and Didactic Framework for Procedural Trainings Using Handheld Augmented Reality](https://www.mdpi.com/2414-4088/5/7/30). Multimodal Technologies and Interaction, 5 (7), 30.

#### TrainAR Trainings:
- Arztmann, M., Domínguez Alfaro, J. L., Hornstra, L., Jeuring, J., & Kester, L. (2023). [In-game performance: The role of students' socio-economic status, self-efficacy and situational interest in an augmented reality game](https://bera-journals.onlinelibrary.wiley.com/action/showCitFormats?doi=10.1111%2Fbjet.13395). British Journal of Educational Technology, 00, 1–15.
- Blattgerste, J., Franssen, J., Arztmann, M., Pfeiffer, T. (2022). [Motivational benefits and usability of a handheld Augmented Reality game for anatomy learning](https://mixality.de/wp-content/uploads/2022/12/Blattgerste2022Motivational.pdf). 2022 IEEE International Conference on Artificial Intelligence and Virtual Reality (AIVR).
- Arztmann, M., Domínguez Alfaro, J. L., Blattgerste, J., Jeuring, J., Van Puyvelde, P. (2022). [Marie’s ChemLab: a Mobile Augmented Reality Game to Teach Basic Chemistry to Children](https://mixality.de/wp-content/uploads/2022/07/Arztmann2022MariesChemLab.pdf). European Conference on Games Based Learning.
- Domínguez Alfaro, J. L., Gantois, S., Blattgerste, J., De Croon, R., Verbert, K., Pfeiffer, T., & Van Puyvelde, P. (2022). [Mobile Augmented Reality Laboratory for Learning Acid–Base Titration](https://pubs.acs.org/doi/abs/10.1021/acs.jchemed.1c00894). Journal of Chemical Education, 99(2), 531–537.
- Blattgerste, J., Vogel, K., Lewa, C., Willmeroth, T., Joswig, M., Schäfer, T., ... & Pfeiffer, T. (2022). [The Heb@ AR App–Five Augmented Reality Trainings for Self-Directed Learning in Academic Midwifery Education](https://mixality.de/wp-content/uploads/2022/07/Blattgerste2022DELFI.pdf). DELFI 2022 – Die 20. Fachtagung Bildungstechnologien der Gesellschaft für Informatik eV.
- Blattgerste, J., Luksch, K., Lewa, C., Kunzendorf, M., Bauer, N. H., Bernloehr, A., ... & Pfeiffer, T. (2020). [Project Heb@AR: Exploring handheld Augmented Reality training to supplement academic midwifery education](https://dl.gi.de/handle/20.500.12116/34147). DELFI 2020 – Die 18. Fachtagung Bildungstechnologien der Gesellschaft für Informatik eV.

## Funding
The research resulting in the open source TrainAR project was partially supported by the grant 16DHB3021, project "HebAR - AR-Based-Training-Technology", by the German Ministry for Education and Research (BMBF) [[1](https://www.wihoforschung.de/wihoforschung/en/bmbf-funding-projects/funding-lines/research-on-digital-higher-education/dritte-foerderlinie-zur-digitalen-hochschulbildung/hebar/hebar_node)] and partially by the Mixed Reality Lab at University of Applied Sciences Emden/Leer. TrainAR is the abstraction and generalization of interaction concepts, feedback mechanisms, and didactic ideas developed during Project Heb@AR [[1](https://mixality.de/hebar/), [2](https://www.hs-gesundheit.de/hebar/uebersicht-hebar)]. Here, procedural Augmented Reality emergency trainings were explored in the academic midwifery education context. During the development it became clear that those concepts could also be generalized towards other training contexts and thus we share TrainAR here. 

## Acknowledgement
TrainAR is freely accessible for commercial and non-commercial use under the MIT license and does not require acknowledgement in your TrainAR training or App itself. If you use TrainAR in the scientific context though, please acknowledge it by citing our publications on the interaction concept, didactic considerations and the authoring tool itself: 

```tex
@article{Blattgerste2023TrainAR,
AUTHOR = {Blattgerste, Jonas and Behrends, Jan and Pfeiffer, Thies},
TITLE = {TrainAR: An Open-Source Visual Scripting-Based Authoring Tool for Procedural Mobile Augmented Reality Trainings},
JOURNAL = {Information},
VOLUME = {14},
YEAR = {2023},
NUMBER = {4},
ARTICLE-NUMBER = {219},
URL = {https://www.mdpi.com/2078-2489/14/4/219},
ISSN = {2078-2489},
DOI = {10.3390/info14040219}
}
```
