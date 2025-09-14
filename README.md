# DailyCheckIn
A website application that helps the employees of Tasfyeh Company (or any other company) to track there check in and check out from anywhare
## Pages
### Login/SignInPage
The default page if the user is not SignedIn or his token duration is expired

the page may look good but its still need some work
1-error handeling in SignIn section
2-some changes in the UI.
3-Add Account Information so the user can change view/edit his information.
4-Add Forget password page.
### Home Page
the default page if the user is signed in.
<img width="1400" height="1050" alt="image" src="https://github.com/user-attachments/assets/1148e55e-1c06-461f-aa58-5b647f6fe4bd" />
1-the user can change the day he is viewing (the default day is the current day) by using the arrows in the most upper section in the page body
2-the user can change that he do the the action that he want to do (checkIn, CheckOut, TimeOff, TimeOffOut) then he press the finger print button that have defferant colores and anmation for example In It will be red if he is not checkedIn yet and then it turn to grean in scanning animation.
3-the user will have struct option to select in the dropdown option so if he is not checkedIn he have only one option is to check in and if he is checked in he will have two option (checkOut, timeOff) and so on.
4-this fingerprint section will only apper if the user is in the current day only.
the page still needs some work:
1-Some better animation
2-rework the buttom section so the user can change is work section in the days that its not the current day.
### Salary Page
this page is important so the user can monter his work week by week and how much did he worked so far.
<img width="1400" height="1050" alt="image" src="https://github.com/user-attachments/assets/115395b8-d5ae-4dd9-8d48-0bbefcefd5cf" />

1-the user can change the weeks and months by the arrows in the upper section of the page body week by week.
2-the month informations will be changed when the month is changed
3-you can see the week's days information in small details and the exclamation mark the user can see if there is any problem that day or his timesOff that day
4-the buttom section shows the hours that he worked that month and the overtime hours and the money he earned by them.
the page still needs some work:
1-the salary must subtract the advances that the user takes.
###Advance Page
this page showed the advances that the user take so far and give him the ability to take advances.
<img width="1400" height="1050" alt="image" src="https://github.com/user-attachments/assets/9315e6a8-7296-45c4-9dcb-eab1a93c260e" />
1-the user can change the months by the upper section of the page body by using the arrows.
2-the user can add advances by using the plus sign and a dialog will new advance dialog will showup
3-the user can edit or delete the advance by the two edit or delete button in the end of the advance row.
## Future work
1-make the changes needed to the pages
2-add arabic language option to the page.
3-Make Admin page that can see the user activity in the website
4-deploy the website
