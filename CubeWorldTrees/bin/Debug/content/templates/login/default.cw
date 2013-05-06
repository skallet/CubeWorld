{$errors}

<input id="hash" type="hidden" value="{$hash}">

<form method='post' action='?do=loginForm-send'>
 <label for='username'>Username:</label>
 <input type='text' name='username' value='{$loginName}' />
 <br />
 <label for='password'>Password:</label>
 <input type='password' name='password' />
 <br />
 <input type='submit' value='Login' />
</form>