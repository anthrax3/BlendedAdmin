# FormView

## Controls
### List of control
 ```javascript
function main(arg)
{
  	var formView = new FormView({
      	controls: [
          {name:'TextBox', label:'Text Box', type:'input'},
          {name:'CheckBox', label:'Check Box', type:'checkbox'},
          {name:'Select1', label:'Select 1', type:'select', options:[1,2,3]},
          {name:'Select1', label:'Select 1', type:'select', options:[{value:1, text:'number 1'},{value:2, text:'number 2'},{value:3, text:'number 3'}]},
          {name:'Date', label:'Date', type:'date'},
          {name:'DateTime', label:'Date and time', type:'datetime'},
        ]
      });
	return [formView];
}
main(arg);
```
![Image of Yaktocat](images/FormView_Controls.PNG)
### Layout
Each element of the controls array is a row.
 ```javascript
function main(arg)
{
  	var formView = new FormView({
      	controls: [
          {name:'Row1'},
          [{name:'Row2_1'},{name:'Row2_2'},{name:'Row2_3'}],
          [{name:'Row3'}, null]
        ]
      });
	return [formView];
}
main(arg);
```
![Image of Yaktocat](images/FormView_Layout.PNG)

## Method
### Get (by default)
Value of the control is sent to the server in query string and can be accessed via *arg.queryString.{name}*
```javascript
function main(arg)
{
  	var formView = new FormView({
      	  method: 'get',
      	  controls: [{name:'TextBox', label:'Text Box', type:'input', value:arg.queryString.TextBox}]
        });
	return [formView];
}
main(arg);
```
![Image of Yaktocat](images/FormView_Method_Get.PNG)

### Post
Value of the control is sent to the server in body and can be accessed via *arg.form.{name}*
 ```javascript
function main(arg)
{
  	var formView = new FormView({
      	  method: 'post',
      	  controls: [{name:'TextBox', label:'Text Box', type:'input', value:arg.form.TextBox}]
        });
	return [formView];
}
main(arg);
```
![Image of Yaktocat](images/FormView_Method_Post.PNG)

## Validation
### Required
```javascript
function main(arg)
{
  	var formView = new FormView({
      	  method: 'post',
      	  controls: [{name:'TextBox', required:true, value:arg.queryString.TextBox}]
        });
  	if (arg.method == 'post' && formView.isValid())
    	{
      	   // do the action
    	}
	return [formView];
}
main(arg);
```
![Image of Yaktocat](images/FormView_Validation_Required.PNG)

### Custom
```javascript
function main(arg)
{
  	var formView = new FormView({
      	  method: 'post',
      	  controls: [{name:'TextBox', value:arg.queryString.TextBox}]
        });
    
  	if (arg.method == 'post')
    	{
   		if (!arg.queryString.TextBox)
    		formView.getControlByName('TextBox').error = 'The field is required by custom validation';
      
		if (formView.isValid())
		{
		  // do the action    
		}
    	}
	return [formView];
}
main(arg);
```
![Image of Yaktocat](images/FormView_Validation_Custom.PNG)

